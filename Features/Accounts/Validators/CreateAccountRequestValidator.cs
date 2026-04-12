using FinancialTracker.API.Features.Accounts.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Accounts.Validators;

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequestDto>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}
