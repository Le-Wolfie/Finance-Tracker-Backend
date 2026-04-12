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
                switch (request.Type)
                {
                    case TransactionType.Income:
                        if (category is not null && category.Type != CategoryType.Income)
                        {
                            throw new BusinessRuleException("Income transaction requires an income category.");
                        }

                        account.Balance += request.Amount;
                        break;
                    case TransactionType.Expense:
                        if (category is not null && category.Type != CategoryType.Expense)
                        {
                            throw new BusinessRuleException("Expense transaction requires an expense category.");
                        }

                        if (account.Balance < request.Amount)
                        {
                            throw new BusinessRuleException("Insufficient account balance.");
                        }

                        account.Balance -= request.Amount;
                        break;
                    case TransactionType.Transfer:
                        if (!request.DestinationAccountId.HasValue)
                        {
                            throw new BusinessRuleException("Destination account is required for transfer.");
                        }

                        destinationAccount = await _dbContext.Accounts
                            .FirstOrDefaultAsync(x => x.Id == request.DestinationAccountId && x.UserId == userId, cancellationToken)
                            ?? throw new NotFoundException("Destination account not found.");

                        if (account.Balance < request.Amount)
                        {
                            throw new BusinessRuleException("Insufficient account balance.");
                        }

                        account.Balance -= request.Amount;
                        destinationAccount.Balance += request.Amount;
                        break;
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

            await using var transactionScope = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Reverse the old transaction effect first.
            switch (existing.Type)
            {
                case TransactionType.Income:
                    existingSourceAccount.Balance -= existing.Amount;
                    break;
                case TransactionType.Expense:
                    existingSourceAccount.Balance += existing.Amount;
                    break;
                case TransactionType.Transfer:
                    if (!existing.DestinationAccountId.HasValue || !accounts.TryGetValue(existing.DestinationAccountId.Value, out var oldDestination))
                    {
                        throw new NotFoundException("Destination account not found.");
                    }

                    existingSourceAccount.Balance += existing.Amount;
                    oldDestination.Balance -= existing.Amount;
                    break;
            }

            Account? newDestinationAccount = null;
            switch (request.Type)
            {
                case TransactionType.Income:
                    if (newCategory is not null && newCategory.Type != CategoryType.Income)
                    {
                        throw new BusinessRuleException("Income transaction requires an income category.");
                    }

                    newSourceAccount.Balance += request.Amount;
                    break;
                case TransactionType.Expense:
                    if (newCategory is not null && newCategory.Type != CategoryType.Expense)
                    {
                        throw new BusinessRuleException("Expense transaction requires an expense category.");
                    }

                    if (newSourceAccount.Balance < request.Amount)
                    {
                        throw new BusinessRuleException("Insufficient account balance.");
                    }

                    newSourceAccount.Balance -= request.Amount;
                    break;
                case TransactionType.Transfer:
                    if (!request.DestinationAccountId.HasValue)
                    {
                        throw new BusinessRuleException("Destination account is required for transfer.");
                    }

                    if (!accounts.TryGetValue(request.DestinationAccountId.Value, out var resolvedDestination))
                    {
                        throw new NotFoundException("Destination account not found.");
                    }

                    newDestinationAccount = resolvedDestination;

                    if (newSourceAccount.Balance < request.Amount)
                    {
                        throw new BusinessRuleException("Insufficient account balance.");
                    }

                    newSourceAccount.Balance -= request.Amount;
                    newDestinationAccount.Balance += request.Amount;
                    break;
            }

            existing.AccountId = request.AccountId;
            existing.DestinationAccountId = request.Type == TransactionType.Transfer ? newDestinationAccount?.Id : null;
            existing.TransferGroupId = request.Type == TransactionType.Transfer
                ? existing.TransferGroupId ?? Guid.NewGuid()
                : null;
            existing.CategoryId = request.CategoryId;
            existing.Amount = request.Amount;
            existing.Type = request.Type;
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

            switch (existing.Type)
            {
                case TransactionType.Income:
                    account.Balance -= existing.Amount;
                    break;
                case TransactionType.Expense:
                    account.Balance += existing.Amount;
                    break;
                case TransactionType.Transfer:
                    if (!existing.DestinationAccountId.HasValue)
                    {
                        throw new BusinessRuleException("Transfer transaction is invalid.");
                    }

                    var destination = await _dbContext.Accounts
                        .FirstOrDefaultAsync(x => x.Id == existing.DestinationAccountId && x.UserId == userId, cancellationToken)
                        ?? throw new NotFoundException("Destination account not found.");

                    account.Balance += existing.Amount;
                    destination.Balance -= existing.Amount;
                    break;
            }

            _dbContext.Transactions.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);
        });
    }

    private static bool IsDuplicateIdempotencyKeyViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("IX_Transactions_UserId_IdempotencyKey", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
                && message.Contains("IdempotencyKey", StringComparison.OrdinalIgnoreCase);
    }
}
