using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Customers;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class CreateCustomerRequestValidatorTests
{
    private readonly CreateCustomerRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNameIsEmpty()
    {
        var result = _validator.TestValidate(new CreateCustomerRequest { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateCustomerRequest { Name = "John Doe", Phone = "123456789" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
