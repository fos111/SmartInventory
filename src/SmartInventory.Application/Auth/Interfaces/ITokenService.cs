using SmartInventory.Domain.Auth.Entities;

namespace SmartInventory.Application.Auth.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateLimitedToken(User user);
}