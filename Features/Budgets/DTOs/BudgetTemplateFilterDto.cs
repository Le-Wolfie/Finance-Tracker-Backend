namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class BudgetTemplateFilterDto
{
    public bool? IsActive { get; set; }
    public Guid? CategoryId { get; set; }
}
