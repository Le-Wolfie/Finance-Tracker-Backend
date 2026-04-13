using FinancialTracker.API.Features.Categories.DTOs;
using FinancialTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.API.Features.Categories;

[ApiController]
[Authorize]
[Route("categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoriesService _categoriesService;
    private readonly IUserContextService _userContextService;

    public CategoriesController(ICategoriesService categoriesService, IUserContextService userContextService)
    {
        _categoriesService = categoriesService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _categoriesService.CreateAsync(request, userId, cancellationToken);
        return Created($"/categories/{result.Id}", result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponseDto>>> Get(CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _categoriesService.GetAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryResponseDto>> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _categoriesService.UpdateAsync(id, request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        await _categoriesService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
