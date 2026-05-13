using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Infrastructure.Auth.Security;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("Status", user.Status.ToString()),
            new("IsEmailVerified", user.IsEmailVerified.ToString())
        };

        if (user.Role.HasValue)
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.IssuerSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateLimitedToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Status", user.Status.ToString()),
            new Claim("limited", "true")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.IssuerSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class JwtSettings
{
    public string IssuerSigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SmartInventory";
    public string Audience { get; set; } = "SmartInventory";
    public int ExpiryMinutes { get; set; } = 60;
}