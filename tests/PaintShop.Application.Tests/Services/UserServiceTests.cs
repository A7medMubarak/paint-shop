using PaintShop.Application.DTOs.Users;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", Role = UserRole.Owner, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" },
            new() { Id = 2, Username = "employee1", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithUniqueUsername_CreatesUser()
    {
        var ctx = MockDbContext.Create();
        var service = new UserService(ctx);

        var result = await service.CreateEmployeeAsync(new CreateEmployeeRequest { Username = "newuser", Password = "Password123" });

        result.Username.Should().Be("newuser");
        result.Role.Should().Be("Employee");
    }

    [Fact]
    public async Task CreateEmployeeAsync_DuplicateUsername_ThrowsException()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "existing", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        var act = () => service.CreateEmployeeAsync(new CreateEmployeeRequest { Username = "existing", Password = "Password123" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Username already exists");
    }

    [Fact]
    public async Task UpdateAsync_WithNewUsername_UpdatesUser()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "oldname", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        var result = await service.UpdateAsync(1, new UpdateUserRequest { Username = "newname" });

        result.Username.Should().Be("newname");
    }

    [Fact]
    public async Task DeactivateAsync_WhenUserExists_SetsInactive()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        await service.DeactivateAsync(1);

        var updated = await ctx.Users.FindAsync(1);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserExists_UpdatesHash()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1") }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        await service.ResetPasswordAsync(1, new ResetPasswordRequest { NewPassword = "NewStrong1" });

        var updated = await ctx.Users.FindAsync(1);
        BCrypt.Net.BCrypt.Verify("NewStrong1", updated!.PasswordHash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("OldPass1", updated.PasswordHash).Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserMissing_ThrowsNotFound()
    {
        var ctx = MockDbContext.Create();
        var service = new UserService(ctx);

        var act = () => service.ResetPasswordAsync(99, new ResetPasswordRequest { NewPassword = "NewStrong1" });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithRole_UpdatesRole()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        var result = await service.UpdateAsync(1, new UpdateUserRequest { Username = "user1", Role = "Owner" });

        result.Role.Should().Be("Owner");
    }

    [Fact]
    public async Task DeactivateAsync_LastActiveOwner_ThrowsException()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", Role = UserRole.Owner, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var ctx = MockDbContext.Create(users: users);
        var service = new UserService(ctx);

        var act = () => service.DeactivateAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot deactivate the last active Owner");
    }
}
