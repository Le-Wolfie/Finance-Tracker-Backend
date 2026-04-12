namespace FinancialTracker.API.Entities;

public sealed class SavingsGoalContribution : BaseEntity
{
    public Guid SavingsGoalId { get; set; }
    public Guid? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public string Note { get; set; } = string.Empty;

    public SavingsGoal? SavingsGoal { get; set; }
    public Transaction? Transaction { get; set; }
}
