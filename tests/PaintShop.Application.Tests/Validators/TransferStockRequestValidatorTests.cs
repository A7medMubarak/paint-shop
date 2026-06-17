using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Inventory;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class TransferStockRequestValidatorTests
{
    private readonly TransferStockRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenSourceEqualsDestination()
    {
        var result = _validator.TestValidate(new TransferStockRequest { ProductVariantId = 1, Quantity = 5, FromLocation = 0, ToLocation = 0 });
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new TransferStockRequest { ProductVariantId = 1, Quantity = 5, FromLocation = 0, ToLocation = 1 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
