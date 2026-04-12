using FinancialTracker.API.Features.RecurringTransactions.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.RecurringTransactions.Validators;

public sealed class RecurringTransactionFilterDtoValidator : AbstractValidator<RecurringTransactionFilterDto>
{
    public RecurringTransactionFilterDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
