using PaintShop.Application.DTOs.Auth;

namespace PaintShop.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
