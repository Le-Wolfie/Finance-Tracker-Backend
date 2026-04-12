namespace FinancialTracker.API.Features.Auth.DTOs;

public sealed class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
