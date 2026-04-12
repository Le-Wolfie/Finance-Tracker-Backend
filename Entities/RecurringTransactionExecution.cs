namespace FinancialTracker.API.Entities;

public sealed class RecurringTransactionExecution : BaseEntity
{
    public Guid RecurringTransactionId { get; set; }
    public DateTime ScheduledForDate { get; set; }
    public Guid? GeneratedTransactionId { get; set; }
    public RecurringExecutionStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; }

    public RecurringTransaction? RecurringTransaction { get; set; }
    public Transaction? GeneratedTransaction { get; set; }
}
