using FinancialTracker.API.Features.Budgets.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Budgets.Validators;

public sealed class ApplyBudgetTemplateRequestValidator : AbstractValidator<ApplyBudgetTemplateRequestDto>
{
    public ApplyBudgetTemplateRequestValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.OverrideMonthlyLimit)
            .GreaterThan(0)
            .When(x => x.OverrideMonthlyLimit.HasValue);
    }
}
