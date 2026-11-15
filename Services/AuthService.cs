using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using a_webapi.Configuration;
using a_webapi.Data;
using a_webapi.DTOs.Auth;
using a_webapi.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace a_webapi.Services;

public class AuthService(AppDbContext dbContext, IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResponseDto> CreateTokenPairAsync(ApplicationUser user)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.ExpirationMinutes);

        // Access Token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Refresh Token
        var refreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = HashToken(refreshToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(
                _jwtOptions.RefreshTokenExpirationDays),
            UserId = user.Id
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAt
        };
    }

    public async Task<LoginResponseDto?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

        var now = DateTime.UtcNow;

        if (storedToken == null ||
            storedToken.RevokedAtUtc != null ||
            storedToken.ExpiresAtUtc <= now ||
            storedToken.User == null)
        {
            return null;
        }

        // Revoke old token
        storedToken.RevokedAtUtc = now;

        return await CreateTokenPairAsync(storedToken.User);
    }

    public async Task RevokeAsync(string refreshToken, string userId)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(t =>
                t.TokenHash == tokenHash &&
                t.UserId == userId);

        if (storedToken == null || storedToken.RevokedAtUtc != null)
            return;
        
        storedToken.RevokedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}