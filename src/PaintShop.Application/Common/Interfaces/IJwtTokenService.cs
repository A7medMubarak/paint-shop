using PaintShop.Domain.Entities;

namespace PaintShop.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
