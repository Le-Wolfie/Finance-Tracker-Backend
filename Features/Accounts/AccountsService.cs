using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Accounts.DTOs;
using FinancialTracker.API.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.Accounts;

public sealed class AccountsService : IAccountsService
{
    private readonly AppDbContext _dbContext;

    public AccountsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccountResponseDto> CreateAsync(CreateAccountRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var account = new Account
        {
            UserId = userId,
            Name = request.Name.Trim(),
            InitialBalance = request.Balance,
            Balance = request.Balance,
            Type = request.Type
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account.Adapt<AccountResponseDto>();
    }

    public async Task<IReadOnlyList<AccountResponseDto>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking() // No tracking since we are only reading data
            .Where(x => x.UserId == userId) // Filter accounts by user ID
            .OrderBy(x => x.Name) // Optional: order accounts by name
            .ProjectToType<AccountResponseDto>() // Use Mapster's ProjectToType to directly project to DTOs at the database level
            .ToListAsync(cancellationToken); // Execute the query and get the results as a list

        return accounts;
    }

    public async Task<AccountReconciliationResponseDto> ReconcileAsync(Guid accountId, Guid userId, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == accountId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        var totalIncome = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.AccountId == accountId && x.Type == TransactionType.Income)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalExpenses = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.AccountId == accountId && x.Type == TransactionType.Expense)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalTransfersOut = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.AccountId == accountId && x.Type == TransactionType.Transfer)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalTransfersIn = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DestinationAccountId == accountId && x.Type == TransactionType.Transfer)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var computedBalance = account.InitialBalance + totalIncome - totalExpenses - totalTransfersOut + totalTransfersIn;
        var difference = account.Balance - computedBalance;
        var isMismatch = Math.Abs(difference) > 0.01m;

        return new AccountReconciliationResponseDto
        {
            AccountId = account.Id,
            AccountName = account.Name,
            InitialBalance = account.InitialBalance,
            StoredBalance = account.Balance,
            ComputedBalance = computedBalance,
            Difference = difference,
            IsMismatch = isMismatch,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            TotalTransfersOut = totalTransfersOut,
            TotalTransfersIn = totalTransfersIn
        };
    }
}
