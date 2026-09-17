using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Users;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public void Should_NotHaveError_WhenUsernameAndRoleValid()
    {
        var result = _validator.TestValidate(new UpdateUserRequest { Username = "emp1", Role = "Employee" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenRoleNull()
    {
        var result = _validator.TestValidate(new UpdateUserRequest { Username = "emp1", Role = null });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_WhenRoleInvalid()
    {
        var result = _validator.TestValidate(new UpdateUserRequest { Username = "emp1", Role = "SuperAdmin" });
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Should_HaveError_WhenUsernameEmpty()
    {
        var result = _validator.TestValidate(new UpdateUserRequest { Username = "", Role = "Owner" });
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
