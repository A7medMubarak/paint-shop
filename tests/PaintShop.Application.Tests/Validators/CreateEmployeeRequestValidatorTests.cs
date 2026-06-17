using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Users;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class CreateEmployeeRequestValidatorTests
{
    private readonly CreateEmployeeRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenPasswordIsTooWeak()
    {
        var result = _validator.TestValidate(new CreateEmployeeRequest { Username = "user", Password = "weak" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateEmployeeRequest { Username = "newuser", Password = "StrongPass1" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
