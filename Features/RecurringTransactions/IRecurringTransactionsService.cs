using FinancialTracker.API.Features.RecurringTransactions.DTOs;
using FinancialTracker.API.Features.Transactions.DTOs;

namespace FinancialTracker.API.Features.RecurringTransactions;

public interface IRecurringTransactionsService
{
    Task<RecurringTransactionResponseDto> CreateAsync(CreateRecurringTransactionRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<RecurringTransactionResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<PagedResultDto<RecurringTransactionResponseDto>> GetAsync(RecurringTransactionFilterDto filter, Guid userId, CancellationToken cancellationToken);
    Task<RecurringTransactionResponseDto> UpdateAsync(Guid id, UpdateRecurringTransactionRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<RecurringTransactionResponseDto> PauseAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<RecurringTransactionResponseDto> ResumeAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<RecurringTransactionExecutionResponseDto> ExecuteNowAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<PagedResultDto<RecurringTransactionExecutionResponseDto>> GetExecutionsAsync(Guid recurringTransactionId, int page, int pageSize, Guid userId, CancellationToken cancellationToken);
    Task<int> ExecuteDueAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
