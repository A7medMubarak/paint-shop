using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Products;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class UpdateProductVariantRequestValidatorTests
{
    private readonly UpdateProductVariantRequestValidator _validator = new();

    [Fact]
    public void Should_NotHaveError_WhenAllNull()
    {
        var result = _validator.TestValidate(new UpdateProductVariantRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_WhenPriceNegative()
    {
        var result = _validator.TestValidate(new UpdateProductVariantRequest { SellingPrice = -5 });
        result.ShouldHaveValidationErrorFor(x => x.SellingPrice!.Value);
    }

    [Fact]
    public void Should_NotHaveError_WhenPricesValid()
    {
        var result = _validator.TestValidate(new UpdateProductVariantRequest { SellingPrice = 50, CostPrice = 30, LowStockThreshold = 5 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
