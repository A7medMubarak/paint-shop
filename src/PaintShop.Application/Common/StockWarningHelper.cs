namespace PaintShop.Application.Common;

public static class StockWarningHelper
{
    public static bool WillGoNegative(decimal currentStock, decimal requested) =>
        currentStock - requested < 0;

    public static bool IsLowStock(decimal quantity, decimal? variantThreshold, decimal systemDefault) =>
        quantity <= (variantThreshold ?? systemDefault);
}
