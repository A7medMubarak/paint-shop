using PaintShop.Application.Common;

namespace PaintShop.Application.Tests.Common;

public class SaleCalculatorTests
{
    [Fact]
    public void Subtotal_WithItems_ReturnsSumOfPriceTimesQuantity()
    {
        var items = new[] { (10m, 2m), (5m, 3m) };
        var result = SaleCalculator.Subtotal(items);
        result.Should().Be(35m);
    }

    [Fact]
    public void Subtotal_WithNoItems_ReturnsZero()
    {
        var result = SaleCalculator.Subtotal([]);
        result.Should().Be(0);
    }

    [Fact]
    public void TotalAmount_WithDiscount_ReturnsSubtotalMinusDiscount()
    {
        var result = SaleCalculator.TotalAmount(100m, 10m);
        result.Should().Be(90m);
    }

    [Fact]
    public void TotalAmount_NegativeDiscount_ThrowsArgumentException()
    {
        var act = () => SaleCalculator.TotalAmount(100m, -5m);
        act.Should().Throw<ArgumentException>().WithMessage("Discount cannot be negative");
    }

    [Fact]
    public void TotalAmount_DiscountExceedsSubtotal_ThrowsArgumentException()
    {
        var act = () => SaleCalculator.TotalAmount(50m, 100m);
        act.Should().Throw<ArgumentException>().WithMessage("Total cannot be negative");
    }

    [Fact]
    public void TotalPerLineDiscount_WithOriginalPrices_ReturnsSumOfDiscounts()
    {
        (decimal? OriginalPrice, decimal UnitPrice, decimal Quantity)[] items = [(10m, 8m, 2m), (20m, 18m, 1m)];
        var result = SaleCalculator.TotalPerLineDiscount(items);
        result.Should().Be(4m);
    }

    [Fact]
    public void TotalPerLineDiscount_WithoutOriginalPrices_ReturnsNull()
    {
        (decimal? OriginalPrice, decimal UnitPrice, decimal Quantity)[] items = [(null, 8m, 2m)];
        var result = SaleCalculator.TotalPerLineDiscount(items);
        result.Should().BeNull();
    }
}
