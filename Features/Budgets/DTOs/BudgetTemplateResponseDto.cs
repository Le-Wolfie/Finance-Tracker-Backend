using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class BudgetTemplateResponseDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public BudgetRolloverStrategy RolloverStrategy { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
