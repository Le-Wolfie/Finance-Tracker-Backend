namespace FinancialTracker.API.Entities;

public sealed class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }

    public User? User { get; set; }
    public ICollection<Transaction> SourceTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> DestinationTransactions { get; set; } = new List<Transaction>();
    public ICollection<RecurringTransaction> SourceRecurringTransactions { get; set; } = new List<RecurringTransaction>();
    public ICollection<RecurringTransaction> DestinationRecurringTransactions { get; set; } = new List<RecurringTransaction>();
    public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
}
