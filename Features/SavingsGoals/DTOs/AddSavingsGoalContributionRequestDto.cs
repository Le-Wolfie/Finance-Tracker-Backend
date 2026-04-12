namespace FinancialTracker.API.Features.SavingsGoals.DTOs;

public sealed class AddSavingsGoalContributionRequestDto
{
    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public Guid? TransactionId { get; set; }
    public string Note { get; set; } = string.Empty;
}
