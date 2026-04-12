using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.RecurringTransactions.DTOs;

public sealed class RecurringTransactionFilterDto
{
    public bool? IsActive { get; set; }
    public RecurrenceFrequency? Frequency { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
