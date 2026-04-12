using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.SavingsGoals.Validators;

public sealed class AddSavingsGoalContributionRequestValidator : AbstractValidator<AddSavingsGoalContributionRequestDto>
{
    public AddSavingsGoalContributionRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.ContributionDate).NotEqual(default(DateTime));
    }
}
