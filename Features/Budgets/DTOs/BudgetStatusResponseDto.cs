namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class BudgetStatusResponseDto
{
    public Guid BudgetId { get; set; }
    public Guid CategoryId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public bool IsExceeded { get; set; }
}
