namespace FinancialTracker.API.Features.Auth.DTOs;

public sealed class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
