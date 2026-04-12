using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class BudgetRolloverRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? FromBudgetId { get; set; }
    public Guid ToBudgetId { get; set; }
    public int FromYear { get; set; }
    public int FromMonth { get; set; }
    public int ToYear { get; set; }
    public int ToMonth { get; set; }
    public decimal PreviousMonthlyLimit { get; set; }
    public decimal PreviousSpent { get; set; }
    public decimal RolledOverAmount { get; set; }
    public decimal NewMonthlyLimit { get; set; }
    public BudgetRolloverStrategy AppliedStrategy { get; set; }
    public DateTime CreatedAt { get; set; }
}
