namespace FinancialTracker.API.Entities;

public sealed class Category : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<BudgetTemplate> BudgetTemplates { get; set; } = new List<BudgetTemplate>();
    public ICollection<BudgetRolloverRecord> BudgetRolloverRecords { get; set; } = new List<BudgetRolloverRecord>();
    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
}
