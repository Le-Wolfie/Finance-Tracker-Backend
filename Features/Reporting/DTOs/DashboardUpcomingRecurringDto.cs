using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Reporting.DTOs;

public sealed class DashboardUpcomingRecurringDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime NextExecutionDate { get; set; }
}
