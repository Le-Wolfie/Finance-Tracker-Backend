using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Transactions.DTOs;

public sealed class TransactionResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public Guid? TransferGroupId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionExecutionMode ExecutionMode { get; set; }
    public bool IsBalanceApplied { get; set; }
    public DateTime? BalanceAppliedAt { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
