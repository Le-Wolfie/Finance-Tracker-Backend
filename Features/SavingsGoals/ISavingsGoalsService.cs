using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FinancialTracker.API.Features.Transactions.DTOs;

namespace FinancialTracker.API.Features.SavingsGoals;

public interface ISavingsGoalsService
{
    Task<SavingsGoalResponseDto> CreateAsync(CreateSavingsGoalRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<PagedResultDto<SavingsGoalResponseDto>> GetAsync(SavingsGoalFilterDto filter, Guid userId, CancellationToken cancellationToken);
    Task<SavingsGoalResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<SavingsGoalResponseDto> UpdateAsync(Guid id, UpdateSavingsGoalRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<SavingsGoalContributionResponseDto> AddContributionAsync(Guid id, AddSavingsGoalContributionRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<PagedResultDto<SavingsGoalContributionResponseDto>> GetContributionsAsync(Guid id, int page, int pageSize, Guid userId, CancellationToken cancellationToken);
    Task<SavingsGoalResponseDto> MarkCompleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<SavingsGoalsSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken);
}
