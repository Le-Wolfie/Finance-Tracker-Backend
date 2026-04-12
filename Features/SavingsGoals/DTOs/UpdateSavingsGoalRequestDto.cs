using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.SavingsGoals.DTOs;

public sealed class UpdateSavingsGoalRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public SavingsGoalStatus Status { get; set; }
}
