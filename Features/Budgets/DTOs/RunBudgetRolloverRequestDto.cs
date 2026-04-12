namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class RunBudgetRolloverRequestDto
{
    public int Year { get; set; }
    public int Month { get; set; }
}
