using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
