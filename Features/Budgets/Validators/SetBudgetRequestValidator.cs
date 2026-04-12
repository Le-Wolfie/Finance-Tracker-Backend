using FinancialTracker.API.Features.Budgets.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Budgets.Validators;

public sealed class SetBudgetRequestValidator : AbstractValidator<SetBudgetRequestDto>
{
    public SetBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.MonthlyLimit)
            .GreaterThan(0);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 3000);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);
    }
}
