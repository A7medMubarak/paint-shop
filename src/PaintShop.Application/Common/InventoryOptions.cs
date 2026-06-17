namespace PaintShop.Application.Common;

public class InventoryOptions
{
    public const string SectionName = "Inventory";
    public decimal DefaultLowStockThreshold { get; set; } = 5.0m;
}
