using FinancialTracker.API.Features.Auth.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
    }
}
