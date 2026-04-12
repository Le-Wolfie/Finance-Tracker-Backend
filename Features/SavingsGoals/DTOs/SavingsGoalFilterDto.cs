using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.SavingsGoals.DTOs;

public sealed class SavingsGoalFilterDto
{
    public SavingsGoalStatus? Status { get; set; }
    public Guid? AccountId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
