namespace FinancialTracker.API.Features.Budgets.DTOs;

public sealed class ApplyBudgetTemplateRequestDto
{
    public Guid TemplateId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? OverrideMonthlyLimit { get; set; }
}
