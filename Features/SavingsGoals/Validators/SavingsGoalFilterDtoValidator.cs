using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.SavingsGoals.Validators;

public sealed class SavingsGoalFilterDtoValidator : AbstractValidator<SavingsGoalFilterDto>
{
    public SavingsGoalFilterDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
