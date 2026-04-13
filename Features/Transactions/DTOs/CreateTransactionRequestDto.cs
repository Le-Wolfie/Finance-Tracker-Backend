using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Transactions.DTOs;

public sealed class CreateTransactionRequestDto
{
    public Guid AccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionExecutionMode ExecutionMode { get; set; } = TransactionExecutionMode.ApplyImmediately;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
}
