using Microsoft.EntityFrameworkCore;
using PaintShop.Application.DTOs.Users;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Entities;
using PaintShop.Domain.Enums;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;

    public UserService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Employee,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task DeactivateAsync(int id)
    {
        var user = await _context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found");

        user.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists");
            user.Username = request.Username;
        }

        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int id, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt
    };
}
