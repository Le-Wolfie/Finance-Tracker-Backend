using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.RecurringTransactions.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.RecurringTransactions.Validators;

public sealed class CreateRecurringTransactionRequestValidator : AbstractValidator<CreateRecurringTransactionRequestDto>
{
    public CreateRecurringTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.StartDate).NotEqual(default(DateTime));

        RuleFor(x => x.IntervalDays)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.Frequency == RecurrenceFrequency.CustomIntervalDays);

        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate.Value >= x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(x => x.MaxOccurrences)
            .GreaterThan(0)
            .When(x => x.MaxOccurrences.HasValue);

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty()
            .When(x => x.Type == TransactionType.Transfer)
            .WithMessage("DestinationAccountId is required for transfer transactions.");

        RuleFor(x => x)
            .Must(x => x.Type != TransactionType.Transfer || x.AccountId != x.DestinationAccountId)
            .WithMessage("Source and destination accounts cannot be the same for transfer transactions.");
    }
}
