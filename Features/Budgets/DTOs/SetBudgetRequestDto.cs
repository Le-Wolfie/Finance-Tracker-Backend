namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class SetBudgetRequestDto
{
    public Guid CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}
