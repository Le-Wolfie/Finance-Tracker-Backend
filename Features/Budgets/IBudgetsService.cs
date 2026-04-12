using FinancialTracker.API.Features.Budgets.DTOs;

namespace FinancialTracker.API.Features.Budgets;

public interface IBudgetsService
{
    Task<BudgetStatusResponseDto> SetAsync(SetBudgetRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<BudgetStatusResponseDto> GetStatusAsync(Guid categoryId, int year, int month, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BudgetAlertDto>> GetAlertsAsync(int year, int month, decimal thresholdPercent, Guid userId, CancellationToken cancellationToken);
    Task<BudgetTemplateResponseDto> CreateTemplateAsync(CreateBudgetTemplateRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BudgetTemplateResponseDto>> GetTemplatesAsync(BudgetTemplateFilterDto filter, Guid userId, CancellationToken cancellationToken);
    Task<BudgetTemplateResponseDto> UpdateTemplateAsync(Guid id, UpdateBudgetTemplateRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<BudgetStatusResponseDto> ApplyTemplateAsync(ApplyBudgetTemplateRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<BudgetRolloverResultDto> RunRolloverAsync(RunBudgetRolloverRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BudgetRolloverRecordResponseDto>> GetRolloverHistoryAsync(int year, int month, Guid userId, CancellationToken cancellationToken);
}
