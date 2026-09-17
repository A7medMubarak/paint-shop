using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Users;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNewPasswordIsTooWeak()
    {
        var result = _validator.TestValidate(new ResetPasswordRequest { NewPassword = "weak" });
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new ResetPasswordRequest { NewPassword = "NewStrong1" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
