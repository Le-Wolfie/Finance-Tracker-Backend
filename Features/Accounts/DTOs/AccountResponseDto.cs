using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Accounts.DTOs;

public sealed class AccountResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
