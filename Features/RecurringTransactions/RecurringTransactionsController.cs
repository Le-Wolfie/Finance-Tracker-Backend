using FinancialTracker.API.Features.RecurringTransactions.DTOs;
using FinancialTracker.API.Features.Transactions.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.RecurringTransactions;

[ApiController]
[Authorize]
[Route("recurring-transactions")]
public sealed class RecurringTransactionsController : ControllerBase
{
    private readonly IRecurringTransactionsService _recurringTransactionsService;
    private readonly IUserContextService _userContextService;

    public RecurringTransactionsController(IRecurringTransactionsService recurringTransactionsService, IUserContextService userContextService)
    {
        _recurringTransactionsService = recurringTransactionsService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecurringTransactionResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecurringTransactionResponseDto>> Create([FromBody] CreateRecurringTransactionRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var recurring = await _recurringTransactionsService.CreateAsync(request, userId, cancellationToken);
        return Created($"/recurring-transactions/{recurring.Id}", recurring);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecurringTransactionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecurringTransactionResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var recurring = await _recurringTransactionsService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(recurring);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<RecurringTransactionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<RecurringTransactionResponseDto>>> Get([FromQuery] RecurringTransactionFilterDto filter, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _recurringTransactionsService.GetAsync(filter, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RecurringTransactionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecurringTransactionResponseDto>> Update([FromRoute] Guid id, [FromBody] UpdateRecurringTransactionRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var recurring = await _recurringTransactionsService.UpdateAsync(id, request, userId, cancellationToken);
        return Ok(recurring);
    }

    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(RecurringTransactionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecurringTransactionResponseDto>> Pause([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var recurring = await _recurringTransactionsService.PauseAsync(id, userId, cancellationToken);
        return Ok(recurring);
    }

    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(RecurringTransactionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecurringTransactionResponseDto>> Resume([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var recurring = await _recurringTransactionsService.ResumeAsync(id, userId, cancellationToken);
        return Ok(recurring);
    }

    [HttpPost("{id:guid}/execute")]
    [ProducesResponseType(typeof(RecurringTransactionExecutionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecurringTransactionExecutionResponseDto>> ExecuteNow([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var execution = await _recurringTransactionsService.ExecuteNowAsync(id, userId, cancellationToken);
        return Ok(execution);
    }

    [HttpGet("{id:guid}/executions")]
    [ProducesResponseType(typeof(PagedResultDto<RecurringTransactionExecutionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<RecurringTransactionExecutionResponseDto>>> Executions(
        [FromRoute] Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();
        var result = await _recurringTransactionsService.GetExecutionsAsync(id, page, pageSize, userId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        await _recurringTransactionsService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
