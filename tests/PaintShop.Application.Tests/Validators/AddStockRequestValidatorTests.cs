using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Inventory;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class AddStockRequestValidatorTests
{
    private readonly AddStockRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenQuantityIsZero()
    {
        var result = _validator.TestValidate(new AddStockRequest { ProductVariantId = 1, Location = 0, Quantity = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new AddStockRequest { ProductVariantId = 1, Location = 0, Quantity = 10 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
