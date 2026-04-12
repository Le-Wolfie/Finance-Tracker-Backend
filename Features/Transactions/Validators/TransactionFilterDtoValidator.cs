using FinancialTracker.API.Features.Transactions.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Transactions.Validators;

public sealed class TransactionFilterDtoValidator : AbstractValidator<TransactionFilterDto>
{
    public TransactionFilterDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
            .WithMessage("From date must be earlier than or equal to To date.");
    }
}
