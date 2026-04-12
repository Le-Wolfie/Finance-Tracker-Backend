using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Auth.DTOs;
using FinancialTracker.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinancialTracker.API.Features.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext dbContext, IOptions<JwtSettings> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException("Email is already registered.");
        }

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken) ?? throw new UnauthorizedAccessException("Invalid email or password.");
        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return BuildAuthResponse(user.Id);
    }

    private AuthResponseDto BuildAuthResponse(Guid userId)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("UserId", userId.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
