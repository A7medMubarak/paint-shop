using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Inventory;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class AdjustStockRequestValidatorTests
{
    private readonly AdjustStockRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenQuantityChangeIsZero()
    {
        var result = _validator.TestValidate(new AdjustStockRequest { ProductVariantId = 1, Location = 0, QuantityChange = 0 });
        result.ShouldHaveValidationErrorFor(x => x.QuantityChange);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new AdjustStockRequest { ProductVariantId = 1, Location = 0, QuantityChange = 5 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
