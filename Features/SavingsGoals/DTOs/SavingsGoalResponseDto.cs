using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.SavingsGoals.DTOs;

public sealed class SavingsGoalResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal CompletionPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public SavingsGoalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
