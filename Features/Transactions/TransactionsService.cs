using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Transactions.DTOs;
using FinancialTracker.API.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.Transactions;

public sealed class TransactionsService : ITransactionsService
{
    private readonly AppDbContext _dbContext;

    public TransactionsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionResponseDto> CreateAsync(CreateTransactionRequestDto request, Guid userId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        //^Creates a resilient execution strategy that can automatically retry database operations
        // in case of transient failures, such as deadlocks or connection issues

        var normalizedIdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : idempotencyKey.Trim();

        if (normalizedIdempotencyKey is not null)
        {
            var existingByKey = await _dbContext.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IdempotencyKey == normalizedIdempotencyKey, cancellationToken);

            if (existingByKey is not null)
            {
                return existingByKey.Adapt<TransactionResponseDto>();
            }
        }

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                var now = DateTime.UtcNow;
                var shouldApplyNow = ShouldApplyNow(request.ExecutionMode, request.Date, now);

                var account = await _dbContext.Accounts
                    .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == userId, cancellationToken)
                    ?? throw new NotFoundException("Account not found.");

                Category? category = null;
                if (request.CategoryId.HasValue)
                {
                    category = await _dbContext.Categories
                        .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userId, cancellationToken)
                        ?? throw new NotFoundException("Category not found.");
                }

                await using var transactionScope = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                //^Begins a new database transaction scope to ensure that all operations within this block are atomic

                Account? destinationAccount = null;

                if (request.Type == TransactionType.Transfer)
                {
                    if (!request.DestinationAccountId.HasValue)
                    {
                        throw new BusinessRuleException("Destination account is required for transfer.");
                    }

                    destinationAccount = await _dbContext.Accounts
                        .FirstOrDefaultAsync(x => x.Id == request.DestinationAccountId && x.UserId == userId, cancellationToken)
                        ?? throw new NotFoundException("Destination account not found.");
                }

                if (shouldApplyNow)
                {
                    ApplyBalanceEffect(request.Type, account, destinationAccount, request.Amount, category, enforceFunds: true);
                }
                else
                {
                    ValidateCategoryForType(category, request.Type);
                }

                var newTransaction = new Transaction
                {
                    UserId = userId,
                    AccountId = account.Id,
                    DestinationAccountId = destinationAccount?.Id,
                    TransferGroupId = request.Type == TransactionType.Transfer ? Guid.NewGuid() : null,
                    IdempotencyKey = normalizedIdempotencyKey,
                    CategoryId = request.CategoryId,
                    Amount = request.Amount,
                    Type = request.Type,
                    ExecutionMode = request.ExecutionMode,
                    IsBalanceApplied = shouldApplyNow,
                    BalanceAppliedAt = shouldApplyNow ? now : null,
                    Date = request.Date,
                    Description = request.Description.Trim()
                };

                _dbContext.Transactions.Add(newTransaction);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transactionScope.CommitAsync(cancellationToken);

                return newTransaction.Adapt<TransactionResponseDto>();
            });
        }
        catch (DbUpdateException ex) when (normalizedIdempotencyKey is not null && IsDuplicateIdempotencyKeyViolation(ex))
        {
            var existingByKey = await _dbContext.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IdempotencyKey == normalizedIdempotencyKey, cancellationToken);

            if (existingByKey is not null)
            {
                return existingByKey.Adapt<TransactionResponseDto>();
            }

            throw;
        }
    }

    public async Task<TransactionResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        return transaction.Adapt<TransactionResponseDto>();
    }

    public async Task<TransactionResponseDto> UpdateAsync(Guid id, UpdateTransactionRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var shouldApplyNow = ShouldApplyNow(request.ExecutionMode, request.Date, now);

            var existing = await _dbContext.Transactions
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
                ?? throw new NotFoundException("Transaction not found.");

            Category? newCategory = null;
            if (request.CategoryId.HasValue)
            {
                newCategory = await _dbContext.Categories
                    .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userId, cancellationToken)
                    ?? throw new NotFoundException("Category not found.");
            }

            var accountIds = new HashSet<Guid> { existing.AccountId, request.AccountId };
            if (existing.DestinationAccountId.HasValue)
            {
                accountIds.Add(existing.DestinationAccountId.Value);
            }

            if (request.DestinationAccountId.HasValue)
            {
                accountIds.Add(request.DestinationAccountId.Value);
            }

            var accounts = await _dbContext.Accounts
                .Where(x => accountIds.Contains(x.Id) && x.UserId == userId)
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            if (!accounts.TryGetValue(existing.AccountId, out var existingSourceAccount))
            {
                throw new NotFoundException("Account not found.");
            }

            if (!accounts.TryGetValue(request.AccountId, out var newSourceAccount))
            {
                throw new NotFoundException("Account not found.");
            }

            Account? oldDestinationAccount = null;
            if (existing.DestinationAccountId.HasValue)
            {
                if (!accounts.TryGetValue(existing.DestinationAccountId.Value, out oldDestinationAccount))
                {
                    throw new NotFoundException("Destination account not found.");
                }
            }

            await using var transactionScope = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (existing.IsBalanceApplied)
            {
                ReverseBalanceEffect(existing.Type, existingSourceAccount, oldDestinationAccount, existing.Amount);
            }

            Account? newDestinationAccount = null;
            if (request.Type == TransactionType.Transfer)
            {
                if (!request.DestinationAccountId.HasValue)
                {
                    throw new BusinessRuleException("Destination account is required for transfer.");
                }

                if (!accounts.TryGetValue(request.DestinationAccountId.Value, out var resolvedDestination))
                {
                    throw new NotFoundException("Destination account not found.");
                }

                newDestinationAccount = resolvedDestination;
            }

            if (shouldApplyNow)
            {
                ApplyBalanceEffect(request.Type, newSourceAccount, newDestinationAccount, request.Amount, newCategory, enforceFunds: true);
            }
            else
            {
                ValidateCategoryForType(newCategory, request.Type);
            }

            existing.AccountId = request.AccountId;
            existing.DestinationAccountId = request.Type == TransactionType.Transfer ? newDestinationAccount?.Id : null;
            existing.TransferGroupId = request.Type == TransactionType.Transfer
                ? existing.TransferGroupId ?? Guid.NewGuid()
                : null;
            existing.CategoryId = request.CategoryId;
            existing.Amount = request.Amount;
            existing.Type = request.Type;
            existing.ExecutionMode = request.ExecutionMode;
            existing.IsBalanceApplied = shouldApplyNow;
            existing.BalanceAppliedAt = shouldApplyNow ? now : null;
            existing.Date = request.Date;
            existing.Description = request.Description.Trim();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);

            return existing.Adapt<TransactionResponseDto>();
        });
    }

    public async Task<PagedResultDto<TransactionResponseDto>> GetAsync(TransactionFilterDto filter, Guid userId, CancellationToken cancellationToken)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (filter.From.HasValue)
        {
            query = query.Where(x => x.Date >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(x => x.Date <= filter.To.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<TransactionResponseDto>()
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TransactionResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            var existing = await _dbContext.Transactions
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
                ?? throw new NotFoundException("Transaction not found.");

            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(x => x.Id == existing.AccountId && x.UserId == userId, cancellationToken)
                ?? throw new NotFoundException("Account not found.");

            await using var transactionScope = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (existing.IsBalanceApplied)
            {
                Account? destination = null;
                if (existing.DestinationAccountId.HasValue)
                {
                    destination = await _dbContext.Accounts
                        .FirstOrDefaultAsync(x => x.Id == existing.DestinationAccountId && x.UserId == userId, cancellationToken)
                        ?? throw new NotFoundException("Destination account not found.");
                }

                ReverseBalanceEffect(existing.Type, account, destination, existing.Amount);
            }

            _dbContext.Transactions.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);
        });
    }

    public async Task<int> ApplyDueTransactionsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dueTransactions = await _dbContext.Transactions
            .Where(x => !x.IsBalanceApplied && x.ExecutionMode == TransactionExecutionMode.ApplyOnDate && x.Date <= now)
            .OrderBy(x => x.Date)
            .Take(100)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var transaction in dueTransactions)
        {
            try
            {
                var sourceAccount = await _dbContext.Accounts
                    .FirstOrDefaultAsync(x => x.Id == transaction.AccountId && x.UserId == transaction.UserId, cancellationToken)
                    ?? throw new NotFoundException("Account not found.");

                Account? destinationAccount = null;
                if (transaction.Type == TransactionType.Transfer)
                {
                    if (!transaction.DestinationAccountId.HasValue)
                    {
                        throw new BusinessRuleException("Destination account is required for transfer.");
                    }

                    destinationAccount = await _dbContext.Accounts
                        .FirstOrDefaultAsync(x => x.Id == transaction.DestinationAccountId && x.UserId == transaction.UserId, cancellationToken)
                        ?? throw new NotFoundException("Destination account not found.");
                }

                Category? category = null;
                if (transaction.CategoryId.HasValue)
                {
                    category = await _dbContext.Categories
                        .FirstOrDefaultAsync(x => x.Id == transaction.CategoryId && x.UserId == transaction.UserId, cancellationToken);
                }

                await using var scope = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                ApplyBalanceEffect(transaction.Type, sourceAccount, destinationAccount, transaction.Amount, category, enforceFunds: true);

                transaction.IsBalanceApplied = true;
                transaction.BalanceAppliedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await scope.CommitAsync(cancellationToken);

                processed++;
            }
            catch
            {
                // Leave transaction pending and retry in the next cycle.
            }
        }

        return processed;
    }

    private static bool IsDuplicateIdempotencyKeyViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("IX_Transactions_UserId_IdempotencyKey", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
                && message.Contains("IdempotencyKey", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldApplyNow(TransactionExecutionMode executionMode, DateTime transactionDate, DateTime now)
    {
        return executionMode == TransactionExecutionMode.ApplyImmediately || transactionDate <= now;
    }

    private static void ValidateCategoryForType(Category? category, TransactionType type)
    {
        if (category is null)
        {
            return;
        }

        if (type == TransactionType.Income && category.Type != CategoryType.Income)
        {
            throw new BusinessRuleException("Income transaction requires an income category.");
        }

        if (type == TransactionType.Expense && category.Type != CategoryType.Expense)
        {
            throw new BusinessRuleException("Expense transaction requires an expense category.");
        }
    }

    private static void ApplyBalanceEffect(
        TransactionType type,
        Account sourceAccount,
        Account? destinationAccount,
        decimal amount,
        Category? category,
        bool enforceFunds)
    {
        ValidateCategoryForType(category, type);

        switch (type)
        {
            case TransactionType.Income:
                sourceAccount.Balance += amount;
                break;
            case TransactionType.Expense:
                if (enforceFunds && sourceAccount.Balance < amount)
                {
                    throw new BusinessRuleException("Insufficient account balance.");
                }

                sourceAccount.Balance -= amount;
                break;
            case TransactionType.Transfer:
                if (destinationAccount is null)
                {
                    throw new BusinessRuleException("Destination account is required for transfer.");
                }

                if (enforceFunds && sourceAccount.Balance < amount)
                {
                    throw new BusinessRuleException("Insufficient account balance.");
                }

                sourceAccount.Balance -= amount;
                destinationAccount.Balance += amount;
                break;
        }
    }

    private static void ReverseBalanceEffect(
        TransactionType type,
        Account sourceAccount,
        Account? destinationAccount,
        decimal amount)
    {
        switch (type)
        {
            case TransactionType.Income:
                sourceAccount.Balance -= amount;
                break;
            case TransactionType.Expense:
                sourceAccount.Balance += amount;
                break;
            case TransactionType.Transfer:
                if (destinationAccount is null)
                {
                    throw new BusinessRuleException("Transfer transaction is invalid.");
                }

                sourceAccount.Balance += amount;
                destinationAccount.Balance -= amount;
                break;
        }
    }
}
