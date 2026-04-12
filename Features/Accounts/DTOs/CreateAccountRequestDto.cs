using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Accounts.DTOs;

public sealed class CreateAccountRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
}
