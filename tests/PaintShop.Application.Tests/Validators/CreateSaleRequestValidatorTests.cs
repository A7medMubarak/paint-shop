using FluentValidation.TestHelper;
using PaintShop.Application.DTOs.Sales;
using PaintShop.Application.Validators;

namespace PaintShop.Application.Tests.Validators;

public class CreateSaleRequestValidatorTests
{
    private readonly CreateSaleRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenItemsIsEmpty()
    {
        var result = _validator.TestValidate(new CreateSaleRequest { CustomerId = 1, Items = [] });
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_HaveError_WhenDiscountIsNegative()
    {
        var result = _validator.TestValidate(new CreateSaleRequest { CustomerId = 1, DiscountAmount = -5, Items = [new() { ProductVariantId = 1, Quantity = 1, UnitPrice = 10 }] });
        result.ShouldHaveValidationErrorFor(x => x.DiscountAmount);
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateSaleRequest
        {
            CustomerId = 1,
            DiscountAmount = 0,
            Items = [new() { ProductVariantId = 1, Quantity = 2, UnitPrice = 50 }]
        });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
