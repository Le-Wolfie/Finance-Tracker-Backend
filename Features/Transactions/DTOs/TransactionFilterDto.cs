namespace FinancialTracker.API.Features.Transactions.DTOs;

public sealed class TransactionFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
