using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.RecurringTransactions.DTOs;

public sealed class RecurringTransactionExecutionResponseDto
{
    public Guid Id { get; set; }
    public Guid RecurringTransactionId { get; set; }
    public DateTime ScheduledForDate { get; set; }
    public Guid? GeneratedTransactionId { get; set; }
    public RecurringExecutionStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; }
}
