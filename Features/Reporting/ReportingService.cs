using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Reporting.DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.Reporting;

public sealed class ReportingService : IReportingService
{
    private readonly AppDbContext _dbContext;

    public ReportingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var totalIncome = await _dbContext.Transactions
            .Where(x => x.UserId == userId && x.Type == TransactionType.Income && x.Date >= start && x.Date < end)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalExpenses = await _dbContext.Transactions
            .Where(x => x.UserId == userId && x.Type == TransactionType.Expense && x.Date >= start && x.Date < end)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return new MonthlySummaryDto
        {
            Year = year,
            Month = month,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetBalance = totalIncome - totalExpenses
        };
    }

    public async Task<IReadOnlyList<CategoryBreakdownDto>> GetCategoryBreakdownAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var breakdown = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && x.Type == TransactionType.Expense
                && x.CategoryId != null
                && x.Date >= start
                && x.Date < end)
            .GroupBy(x => new { x.CategoryId, CategoryName = x.Category != null ? x.Category.Name : "Uncategorized" })
            .Select(x => new CategoryBreakdownDto
            {
                CategoryId = x.Key.CategoryId!.Value,
                CategoryName = x.Key.CategoryName,
                Amount = x.Sum(y => y.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);

        return breakdown;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(int year, int month, decimal budgetAlertThresholdPercent, Guid userId, CancellationToken cancellationToken)
    {
        var summary = await GetMonthlySummaryAsync(year, month, userId, cancellationToken);

        var topCategories = await GetCategoryBreakdownAsync(year, month, userId, cancellationToken);
        var topFiveCategories = topCategories.Take(5).ToList();

        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ProjectToType<DashboardAccountDto>()
            .ToListAsync(cancellationToken);

        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var normalizedThreshold = Math.Clamp(budgetAlertThresholdPercent, 0, 100);

        var budgets = await _dbContext.Budgets
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Year == year && x.Month == month)
            .Select(x => new
            {
                x.Id,
                x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : "Uncategorized",
                x.MonthlyLimit
            })
            .ToListAsync(cancellationToken);

        var alerts = new List<DashboardBudgetAlertDto>();
        if (budgets.Count > 0)
        {
            var categoryIds = budgets.Select(x => x.CategoryId).ToList();
            var spendingByCategory = await _dbContext.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && x.Type == TransactionType.Expense
                    && x.CategoryId.HasValue
                    && categoryIds.Contains(x.CategoryId.Value)
                    && x.Date >= start
                    && x.Date < end)
                .GroupBy(x => x.CategoryId!.Value)
                .Select(x => new { CategoryId = x.Key, Spent = x.Sum(y => y.Amount) })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Spent, cancellationToken);

            alerts = budgets
                .Select(x =>
                {
                    var spent = spendingByCategory.GetValueOrDefault(x.CategoryId, 0m);
                    var usagePercent = x.MonthlyLimit <= 0 ? 0 : (spent / x.MonthlyLimit) * 100;
                    var remaining = x.MonthlyLimit - spent;
                    return new DashboardBudgetAlertDto
                    {
                        BudgetId = x.Id,
                        CategoryId = x.CategoryId,
                        CategoryName = x.CategoryName,
                        MonthlyLimit = x.MonthlyLimit,
                        Spent = spent,
                        Remaining = remaining,
                        UsagePercent = Math.Round(usagePercent, 2),
                        IsExceeded = remaining < 0
                    };
                })
                .Where(x => x.IsExceeded || x.UsagePercent >= normalizedThreshold)
                .OrderByDescending(x => x.UsagePercent)
                .ToList();
        }

        var goals = await _dbContext.SavingsGoals
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var goalsTarget = goals.Sum(x => x.TargetAmount);
        var goalsCurrent = goals.Sum(x => x.CurrentAmount);
        var goalsSummary = new DashboardSavingsGoalsSummaryDto
        {
            ActiveGoalsCount = goals.Count(x => x.Status == SavingsGoalStatus.Active),
            CompletedGoalsCount = goals.Count(x => x.Status == SavingsGoalStatus.Completed),
            TotalTargetAmount = goalsTarget,
            TotalCurrentAmount = goalsCurrent,
            OverallCompletionPercent = goalsTarget <= 0 ? 0 : Math.Round((goalsCurrent / goalsTarget) * 100m, 2)
        };

        var upcomingRecurring = await _dbContext.RecurringTransactions
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && x.IsActive
                && x.NextExecutionDate >= start
                && x.NextExecutionDate < end)
            .OrderBy(x => x.NextExecutionDate)
            .Take(5)
            .Select(x => new DashboardUpcomingRecurringDto
            {
                Id = x.Id,
                Name = x.Name,
                Amount = x.Amount,
                Type = x.Type,
                NextExecutionDate = x.NextExecutionDate
            })
            .ToListAsync(cancellationToken);

        return new DashboardOverviewDto
        {
            Summary = summary,
            TopExpenseCategories = topFiveCategories,
            Accounts = accounts,
            BudgetAlerts = alerts,
            SavingsGoals = goalsSummary,
            UpcomingRecurringTransactions = upcomingRecurring
        };
    }
}
