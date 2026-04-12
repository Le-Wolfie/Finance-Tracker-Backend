using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.RecurringTransactions.DTOs;

public sealed class CreateRecurringTransactionRequestDto
{
    public Guid AccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int? IntervalDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxOccurrences { get; set; }
}
