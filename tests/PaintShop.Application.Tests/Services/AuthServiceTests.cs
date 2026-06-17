using PaintShop.Application.Common.Interfaces;
using PaintShop.Application.DTOs.Auth;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class AuthServiceTests
{
    private static User CreateTestUser(int id = 1, string username = "admin", string password = "Admin123", UserRole role = UserRole.Owner, bool isActive = true)
    {
        return new User
        {
            Id = id,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            IsActive = isActive,
            CreatedAt = DateTime.Now
        };
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var users = new List<User> { CreateTestUser() };
        var ctx = MockDbContext.Create(users: users);

        var jwtMock = new Mock<IJwtTokenService>();
        jwtMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("test-token");

        var service = new AuthService(ctx, jwtMock.Object);

        var result = await service.LoginAsync(new LoginRequest { Username = "admin", Password = "Admin123" });

        result.Token.Should().Be("test-token");
        result.Role.Should().Be("Owner");
        jwtMock.Verify(x => x.GenerateToken(It.Is<User>(u => u.Username == "admin")), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var users = new List<User> { CreateTestUser() };
        var ctx = MockDbContext.Create(users: users);
        var jwtMock = new Mock<IJwtTokenService>();
        var service = new AuthService(ctx, jwtMock.Object);

        var act = () => service.LoginAsync(new LoginRequest { Username = "admin", Password = "WrongPassword" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid username or password");
        jwtMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithWrongUsername_ThrowsUnauthorizedAccessException()
    {
        var users = new List<User> { CreateTestUser() };
        var ctx = MockDbContext.Create(users: users);
        var jwtMock = new Mock<IJwtTokenService>();
        var service = new AuthService(ctx, jwtMock.Object);

        var act = () => service.LoginAsync(new LoginRequest { Username = "nonexistent", Password = "Admin123" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid username or password");
    }
}
