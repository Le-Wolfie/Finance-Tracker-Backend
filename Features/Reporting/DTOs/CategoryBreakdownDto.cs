namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class CategoryBreakdownDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
