using FinancialTracker.API.Features.Reporting.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.Reporting;

[ApiController]
[Authorize]
[Route("reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly IUserContextService _userContextService;

    public ReportsController(IReportingService reportingService, IUserContextService userContextService)
    {
        _reportingService = reportingService;
        _userContextService = userContextService;
    }

    [HttpGet("monthly-summary")]
    [ProducesResponseType(typeof(MonthlySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonthlySummaryDto>> MonthlySummary([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var summary = await _reportingService.GetMonthlySummaryAsync(year, month, userId, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("category-breakdown")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryBreakdownDto>>> CategoryBreakdown([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var breakdown = await _reportingService.GetCategoryBreakdownAsync(year, month, userId, cancellationToken);
        return Ok(breakdown);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardOverviewDto>> Dashboard(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] decimal budgetAlertThresholdPercent = 80,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();
        var result = await _reportingService.GetDashboardOverviewAsync(year, month, budgetAlertThresholdPercent, userId, cancellationToken);
        return Ok(result);
    }
}
