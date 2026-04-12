namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class DashboardSavingsGoalsSummaryDto
{
    public int ActiveGoalsCount { get; set; }
    public int CompletedGoalsCount { get; set; }
    public decimal TotalTargetAmount { get; set; }
    public decimal TotalCurrentAmount { get; set; }
    public decimal OverallCompletionPercent { get; set; }
}
