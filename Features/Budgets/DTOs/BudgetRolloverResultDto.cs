namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class BudgetRolloverResultDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int ProcessedCount { get; set; }
    public int SkippedCount { get; set; }
}
