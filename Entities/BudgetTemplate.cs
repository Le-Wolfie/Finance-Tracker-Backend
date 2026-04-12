namespace FinancialTracker.API.Entities;

public sealed class BudgetTemplate : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public BudgetRolloverStrategy RolloverStrategy { get; set; } = BudgetRolloverStrategy.None;
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public Category? Category { get; set; }
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
