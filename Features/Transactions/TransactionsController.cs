using FinancialTracker.API.Features.Transactions.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.Transactions;

[ApiController]
[Authorize]
[Route("transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionsService _transactionsService;
    private readonly IUserContextService _userContextService;

    public TransactionsController(ITransactionsService transactionsService, IUserContextService userContextService)
    {
        _transactionsService = transactionsService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TransactionResponseDto>> Create(
        [FromBody] CreateTransactionRequestDto request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var transaction = await _transactionsService.CreateAsync(request, userId, idempotencyKey, cancellationToken);
        return Created($"/transactions/{transaction.Id}", transaction);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionResponseDto>> Update([FromRoute] Guid id, [FromBody] UpdateTransactionRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var transaction = await _transactionsService.UpdateAsync(id, request, userId, cancellationToken);
        return Ok(transaction);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var transaction = await _transactionsService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(transaction);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TransactionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TransactionResponseDto>>> Get([FromQuery] TransactionFilterDto filter, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var results = await _transactionsService.GetAsync(filter, userId, cancellationToken);
        return Ok(results);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        await _transactionsService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
