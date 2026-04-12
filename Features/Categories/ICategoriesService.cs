using FinancialTracker.API.Features.Categories.DTOs;

namespace FinancialTracker.API.Features.Categories;

public interface ICategoriesService
{
    Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponseDto>> GetAsync(Guid userId, CancellationToken cancellationToken);
}
