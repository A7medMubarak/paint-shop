using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Users;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNewPasswordIsTooWeak()
    {
        var result = _validator.TestValidate(new ChangePasswordRequest { CurrentPassword = "old", NewPassword = "weak" });
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new ChangePasswordRequest { CurrentPassword = "OldPass1", NewPassword = "NewStrong1" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
