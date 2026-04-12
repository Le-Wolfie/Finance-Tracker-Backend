using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Categories.DTOs;
using FinancialTracker.API.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.Categories;

public sealed class CategoriesService : ICategoriesService
{
    private readonly AppDbContext _dbContext;

    public CategoriesService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Categories.AnyAsync(
            x => x.UserId == userId && x.Name == request.Name && x.Type == request.Type,
            cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException("Category already exists.");
        }

        var category = new Category
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return category.Adapt<CategoryResponseDto>();
    }

    public async Task<IReadOnlyList<CategoryResponseDto>> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ProjectToType<CategoryResponseDto>()
            .ToListAsync(cancellationToken);
    }
}
