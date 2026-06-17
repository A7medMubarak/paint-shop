namespace PaintShop.Application.Common;

public static class SaleCalculator
{
    public static decimal Subtotal(IEnumerable<(decimal UnitPrice, decimal Quantity)> items) =>
        items.Sum(i => i.UnitPrice * i.Quantity);

    public static decimal TotalAmount(decimal subtotal, decimal discount)
    {
        if (discount < 0) throw new ArgumentException("Discount cannot be negative");
        var total = subtotal - discount;
        if (total < 0) throw new ArgumentException("Total cannot be negative");
        return total;
    }

    public static decimal? TotalPerLineDiscount(
        IEnumerable<(decimal? OriginalPrice, decimal UnitPrice, decimal Quantity)> items)
    {
        var hasReference = items.Any(i => i.OriginalPrice.HasValue);
        if (!hasReference) return null;
        return items.Sum(i => (i.OriginalPrice ?? i.UnitPrice) - i.UnitPrice);
    }
}
