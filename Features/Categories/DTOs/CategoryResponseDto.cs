using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Categories.DTOs;

public sealed class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
}
