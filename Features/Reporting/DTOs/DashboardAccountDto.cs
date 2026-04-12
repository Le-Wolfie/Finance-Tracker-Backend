using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class DashboardAccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
}
