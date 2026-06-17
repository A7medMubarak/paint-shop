using Microsoft.EntityFrameworkCore;
using PaintShop.Application.DTOs.Auth;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Interfaces;
using PaintShop.Application.Common.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username)
            ?? throw new UnauthorizedAccessException("Invalid username or password");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            Role = user.Role.ToString(),
            Username = user.Username
        };
    }
}
