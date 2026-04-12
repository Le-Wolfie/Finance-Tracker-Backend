using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FinancialTracker.API.Services;

public sealed class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor; // Used to access the current HTTP context and retrieve user information

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

/// <summary>
/// Retrieves the authenticated user's ID from the JWT claims.
/// </summary>
/// <returns></returns>
/// <exception cref="UnauthorizedAccessException"></exception> 
    public Guid GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("UserId");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("UserId claim is missing or invalid.");
        }

        return userId;
    }
}
