namespace FinancialTracker.API.Features.SavingsGoals.DTOs;

public sealed class CreateSavingsGoalRequestDto
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
}
