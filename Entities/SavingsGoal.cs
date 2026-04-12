namespace FinancialTracker.API.Entities;

public sealed class SavingsGoal : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public SavingsGoalStatus Status { get; set; } = SavingsGoalStatus.Active;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public ICollection<SavingsGoalContribution> Contributions { get; set; } = new List<SavingsGoalContribution>();
}
