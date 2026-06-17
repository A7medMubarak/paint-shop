using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Products;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenCategoryIsOutOfRange()
    {
        var result = _validator.TestValidate(new CreateProductRequest { Name = "Paint", ProductCategory = 5 });
        result.ShouldHaveValidationErrorFor(x => x.ProductCategory);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateProductRequest { Name = "Paint", ProductCategory = 0 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
