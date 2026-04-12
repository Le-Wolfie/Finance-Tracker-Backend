using FinancialTracker.API.Features.Accounts.DTOs;

namespace FinancialTracker.API.Features.Accounts;

public interface IAccountsService
{
    Task<AccountResponseDto> CreateAsync(CreateAccountRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccountResponseDto>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<AccountReconciliationResponseDto> ReconcileAsync(Guid accountId, Guid userId, CancellationToken cancellationToken);
}
