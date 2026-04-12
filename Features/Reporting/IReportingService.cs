using FinancialTracker.API.Features.Reporting.DTOs;

namespace FinancialTracker.API.Features.Reporting;

public interface IReportingService
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryBreakdownDto>> GetCategoryBreakdownAsync(int year, int month, Guid userId, CancellationToken cancellationToken);
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(int year, int month, decimal budgetAlertThresholdPercent, Guid userId, CancellationToken cancellationToken);
}
