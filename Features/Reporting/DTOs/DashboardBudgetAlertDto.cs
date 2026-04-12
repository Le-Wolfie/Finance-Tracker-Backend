namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class DashboardBudgetAlertDto
{
    public Guid BudgetId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal UsagePercent { get; set; }
    public bool IsExceeded { get; set; }
}
