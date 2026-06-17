using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Auth;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenUsernameIsEmpty()
    {
        var result = _validator.TestValidate(new LoginRequest { Username = "", Password = "pass" });
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new LoginRequest { Username = "admin", Password = "pass" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
