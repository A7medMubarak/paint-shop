using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Sales;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class CreateSaleItemRequestValidatorTests
{
    private readonly CreateSaleItemRequestValidator _validator = new();

    [Fact]
    public void Should_NotHaveError_WhenItemValid()
    {
        var result = _validator.TestValidate(new CreateSaleItemRequest { ProductVariantId = 1, Quantity = 2, UnitPrice = 50 });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_WhenQuantityZero()
    {
        var result = _validator.TestValidate(new CreateSaleItemRequest { ProductVariantId = 1, Quantity = 0, UnitPrice = 50 });
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_HaveError_WhenVariantIdInvalid()
    {
        var result = _validator.TestValidate(new CreateSaleItemRequest { ProductVariantId = 0, Quantity = 1, UnitPrice = 50 });
        result.ShouldHaveValidationErrorFor(x => x.ProductVariantId);
    }
}
