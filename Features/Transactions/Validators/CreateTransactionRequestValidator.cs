using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Transactions.DTOs;
using FluentValidation;

namespace FinancialTracker.API.Features.Transactions.Validators;

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequestDto>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Date)
            .NotEqual(default(DateTime));

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty()
            .When(x => x.Type == TransactionType.Transfer)
            .WithMessage("DestinationAccountId is required for transfer transactions.");

        RuleFor(x => x)
            .Must(x => x.Type != TransactionType.Transfer || x.AccountId != x.DestinationAccountId)
            .WithMessage("Source and destination accounts cannot be the same for transfer transactions.");
    }
}
