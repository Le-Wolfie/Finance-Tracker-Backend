using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class UpdateBudgetTemplateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public BudgetRolloverStrategy RolloverStrategy { get; set; } = BudgetRolloverStrategy.None;
    public bool IsActive { get; set; } = true;
}
