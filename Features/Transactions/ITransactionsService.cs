using FinancialTracker.API.Features.Transactions.DTOs;

namespace FinancialTracker.API.Features.Transactions;

public interface ITransactionsService
{
    Task<TransactionResponseDto> CreateAsync(CreateTransactionRequestDto request, Guid userId, string? idempotencyKey, CancellationToken cancellationToken);
    Task<TransactionResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<TransactionResponseDto> UpdateAsync(Guid id, UpdateTransactionRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<PagedResultDto<TransactionResponseDto>> GetAsync(TransactionFilterDto filter, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<int> ApplyDueTransactionsAsync(CancellationToken cancellationToken);
}
