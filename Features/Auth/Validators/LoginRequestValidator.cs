using FinancialTracker.API.Features.Auth.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
