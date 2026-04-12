using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FinancialTracker.API.Features.Transactions.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.SavingsGoals;

[ApiController]
[Authorize]
[Route("savings-goals")]
public sealed class SavingsGoalsController : ControllerBase
{
    private readonly ISavingsGoalsService _savingsGoalsService;
    private readonly IUserContextService _userContextService;

    public SavingsGoalsController(ISavingsGoalsService savingsGoalsService, IUserContextService userContextService)
    {
        _savingsGoalsService = savingsGoalsService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SavingsGoalResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SavingsGoalResponseDto>> Create([FromBody] CreateSavingsGoalRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.CreateAsync(request, userId, cancellationToken);
        return Created($"/savings-goals/{result.Id}", result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<SavingsGoalResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<SavingsGoalResponseDto>>> Get([FromQuery] SavingsGoalFilterDto filter, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.GetAsync(filter, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SavingsGoalResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SavingsGoalResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SavingsGoalResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SavingsGoalResponseDto>> Update([FromRoute] Guid id, [FromBody] UpdateSavingsGoalRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.UpdateAsync(id, request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/contributions")]
    [ProducesResponseType(typeof(SavingsGoalContributionResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SavingsGoalContributionResponseDto>> AddContribution([FromRoute] Guid id, [FromBody] AddSavingsGoalContributionRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.AddContributionAsync(id, request, userId, cancellationToken);
        return Created($"/savings-goals/{id}/contributions/{result.Id}", result);
    }

    [HttpGet("{id:guid}/contributions")]
    [ProducesResponseType(typeof(PagedResultDto<SavingsGoalContributionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<SavingsGoalContributionResponseDto>>> GetContributions(
        [FromRoute] Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.GetContributionsAsync(id, page, pageSize, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-complete")]
    [ProducesResponseType(typeof(SavingsGoalResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SavingsGoalResponseDto>> MarkComplete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.MarkCompleteAsync(id, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(SavingsGoalsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SavingsGoalsSummaryDto>> Summary(CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _savingsGoalsService.GetSummaryAsync(userId, cancellationToken);
        return Ok(result);
    }
}
