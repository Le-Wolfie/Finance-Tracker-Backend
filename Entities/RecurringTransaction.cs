namespace FinancialTracker.API.Entities;

public sealed class RecurringTransaction : BaseEntity
{
    public Guid UserId { get; set; }
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
    public DateTime NextExecutionDate { get; set; }
    public int ExecutedCount { get; set; }
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public Account? DestinationAccount { get; set; }
    public Category? Category { get; set; }
    public ICollection<RecurringTransactionExecution> Executions { get; set; } = new List<RecurringTransactionExecution>();
}
