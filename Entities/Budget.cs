namespace FinancialTracker.API.Entities;

public sealed class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? TemplateId { get; set; }
    public Guid? RolloverFromBudgetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal MonthlyLimit { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
    public BudgetTemplate? Template { get; set; }
    public Budget? RolloverFromBudget { get; set; }
    public ICollection<Budget> RolledOverToBudgets { get; set; } = new List<Budget>();
}
