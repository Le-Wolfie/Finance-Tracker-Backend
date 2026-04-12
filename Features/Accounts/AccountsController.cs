using FinancialTracker.API.Features.Accounts.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.Accounts;

[ApiController]
[Authorize]
[Route("accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountsService _accountsService;
    private readonly IUserContextService _userContextService;

    public AccountsController(IAccountsService accountsService, IUserContextService userContextService)
    {
        _accountsService = accountsService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountResponseDto>> Create([FromBody] CreateAccountRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var account = await _accountsService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountResponseDto>>> Get(CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var accounts = await _accountsService.ListAsync(userId, cancellationToken);
        return Ok(accounts);
    }

    [HttpGet("{id:guid}/reconcile")]
    [ProducesResponseType(typeof(AccountReconciliationResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountReconciliationResponseDto>> Reconcile([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var reconciliation = await _accountsService.ReconcileAsync(id, userId, cancellationToken);
        return Ok(reconciliation);
    }
}
