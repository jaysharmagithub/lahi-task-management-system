using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    Guid? GetUserIdFromToken(string token);
}
