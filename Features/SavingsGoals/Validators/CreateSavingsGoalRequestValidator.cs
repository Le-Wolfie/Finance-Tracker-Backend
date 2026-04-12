using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.SavingsGoals.Validators;

public sealed class CreateSavingsGoalRequestValidator : AbstractValidator<CreateSavingsGoalRequestDto>
{
    public CreateSavingsGoalRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEqual(default(DateTime));
        RuleFor(x => x.TargetDate).NotEqual(default(DateTime));

        RuleFor(x => x)
            .Must(x => x.TargetDate >= x.StartDate)
            .WithMessage("TargetDate must be greater than or equal to StartDate.");
    }
}
