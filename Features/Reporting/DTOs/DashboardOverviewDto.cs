namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class DashboardOverviewDto
{
    public MonthlySummaryDto Summary { get; set; } = new();
    public IReadOnlyList<CategoryBreakdownDto> TopExpenseCategories { get; set; } = [];
    public IReadOnlyList<DashboardAccountDto> Accounts { get; set; } = [];
    public IReadOnlyList<DashboardBudgetAlertDto> BudgetAlerts { get; set; } = [];
    public DashboardSavingsGoalsSummaryDto SavingsGoals { get; set; } = new();
    public IReadOnlyList<DashboardUpcomingRecurringDto> UpcomingRecurringTransactions { get; set; } = [];
}
