using FinancialTracker.API.Features.Budgets.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.Budgets;

[ApiController]
[Authorize]
[Route("budgets")]
public sealed class BudgetsController : ControllerBase
{
    private readonly IBudgetsService _budgetsService;
    private readonly IUserContextService _userContextService;

    public BudgetsController(IBudgetsService budgetsService, IUserContextService userContextService)
    {
        _budgetsService = budgetsService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetStatusResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetStatusResponseDto>> Set([FromBody] SetBudgetRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.SetAsync(request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(BudgetStatusResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetStatusResponseDto>> Status([FromQuery] Guid categoryId, [FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.GetStatusAsync(categoryId, year, month, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BudgetAlertDto>>> Alerts(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] decimal thresholdPercent = 80,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();
        var alerts = await _budgetsService.GetAlertsAsync(year, month, thresholdPercent, userId, cancellationToken);
        return Ok(alerts);
    }

    [HttpPost("templates")]
    [ProducesResponseType(typeof(BudgetTemplateResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BudgetTemplateResponseDto>> CreateTemplate([FromBody] CreateBudgetTemplateRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.CreateTemplateAsync(request, userId, cancellationToken);
        return Created($"/budgets/templates/{result.Id}", result);
    }

    [HttpGet("templates")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetTemplateResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BudgetTemplateResponseDto>>> Templates([FromQuery] BudgetTemplateFilterDto filter, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.GetTemplatesAsync(filter, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("templates/{id:guid}")]
    [ProducesResponseType(typeof(BudgetTemplateResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetTemplateResponseDto>> UpdateTemplate([FromRoute] Guid id, [FromBody] UpdateBudgetTemplateRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.UpdateTemplateAsync(id, request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("apply-template")]
    [ProducesResponseType(typeof(BudgetStatusResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetStatusResponseDto>> ApplyTemplate([FromBody] ApplyBudgetTemplateRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.ApplyTemplateAsync(request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("rollover")]
    [ProducesResponseType(typeof(BudgetRolloverResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BudgetRolloverResultDto>> RunRollover([FromBody] RunBudgetRolloverRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.RunRolloverAsync(request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("rollover/history")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetRolloverRecordResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BudgetRolloverRecordResponseDto>>> RolloverHistory(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();
        var result = await _budgetsService.GetRolloverHistoryAsync(year, month, userId, cancellationToken);
        return Ok(result);
    }
}
