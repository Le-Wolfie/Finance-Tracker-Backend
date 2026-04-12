using FinancialTracker.API.Features.Budgets.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Budgets.Validators;

public sealed class RunBudgetRolloverRequestValidator : AbstractValidator<RunBudgetRolloverRequestDto>
{
    public RunBudgetRolloverRequestValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
