namespace FinancialTracker.API.Features.SavingsGoals.DTOs;

public sealed class SavingsGoalContributionResponseDto
{
    public Guid Id { get; set; }
    public Guid SavingsGoalId { get; set; }
    public Guid? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
