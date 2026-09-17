using PaintShop.Application.DTOs.Users;

namespace PaintShop.Application.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> CreateEmployeeAsync(CreateEmployeeRequest request);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request);
    Task ChangePasswordAsync(int id, ChangePasswordRequest request);
    Task ResetPasswordAsync(int id, ResetPasswordRequest request);
    Task DeactivateAsync(int id);
}
