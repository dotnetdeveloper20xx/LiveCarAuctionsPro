using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CarAuctions.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        var secretKey = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("isDealer", user.IsDealer.ToString().ToLower()),
            new("kycVerified", user.IsKycVerified.ToString().ToLower())
        };

        // Add roles as claims
        foreach (var role in Enum.GetValues<UserRole>())
        {
            if (role != UserRole.None && user.Roles.HasFlag(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }
        }

        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "60");
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateRefreshToken(string token)
    {
        // In a production system, you would validate against stored refresh tokens
        // For now, we just check it's not empty and has a valid format
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(token);
            return bytes.Length == 64;
        }
        catch
        {
            return false;
        }
    }
}
