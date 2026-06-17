using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Products;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class CreateProductVariantRequestValidatorTests
{
    private readonly CreateProductVariantRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenSizeValueIsZero()
    {
        var result = _validator.TestValidate(new CreateProductVariantRequest { SizeValue = 0, SizeUnit = "L" });
        result.ShouldHaveValidationErrorFor(x => x.SizeValue);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateProductVariantRequest
        {
            SizeValue = 1,
            SizeUnit = "L",
            BaseType = 0,
            SellingPrice = 100,
            CostPrice = 50
        });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
