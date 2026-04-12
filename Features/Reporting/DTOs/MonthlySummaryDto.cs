namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance { get; set; }
}
