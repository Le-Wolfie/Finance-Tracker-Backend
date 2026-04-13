using FinancialTracker.API.Features.Categories.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Categories.Validators;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequestDto>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}
