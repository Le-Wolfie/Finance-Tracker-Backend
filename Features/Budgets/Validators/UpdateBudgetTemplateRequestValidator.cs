using FinancialTracker.API.Features.Budgets.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Budgets.Validators;

public sealed class UpdateBudgetTemplateRequestValidator : AbstractValidator<UpdateBudgetTemplateRequestDto>
{
    public UpdateBudgetTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.RolloverStrategy).IsInEnum();
    }
}
