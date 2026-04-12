using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.SavingsGoals.Validators;

public sealed class UpdateSavingsGoalRequestValidator : AbstractValidator<UpdateSavingsGoalRequestDto>
{
    public UpdateSavingsGoalRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.TargetDate).NotEqual(default(DateTime));
        RuleFor(x => x.Status).IsInEnum();
    }
}
