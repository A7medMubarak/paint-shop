namespace PaintShop.Application.DTOs.Inventory;

public class InventoryItemDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = string.Empty;
    public decimal ShopStock { get; set; }
    public decimal WarehouseStock { get; set; }
    public bool IsLowStock { get; set; }
}
