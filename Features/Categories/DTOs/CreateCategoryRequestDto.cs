using FinancialTracker.API.Entities;

namespace FinancialTracker.API.Features.Categories.DTOs;

public sealed class CreateCategoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
}
