namespace FinancialTracker.API.Entities;

public sealed class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public Guid? TransferGroupId { get; set; }
    public string? IdempotencyKey { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public Account? DestinationAccount { get; set; }
    public Category? Category { get; set; }
    public ICollection<RecurringTransactionExecution> RecurringExecutions { get; set; } = new List<RecurringTransactionExecution>();
    public ICollection<SavingsGoalContribution> SavingsGoalContributions { get; set; } = new List<SavingsGoalContribution>();
}
