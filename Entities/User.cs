namespace FinancialTracker.API.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<BudgetTemplate> BudgetTemplates { get; set; } = new List<BudgetTemplate>();
    public ICollection<BudgetRolloverRecord> BudgetRolloverRecords { get; set; } = new List<BudgetRolloverRecord>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
    public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
}
