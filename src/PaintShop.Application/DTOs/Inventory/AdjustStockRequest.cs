namespace PaintShop.Application.DTOs.Inventory;

public class AdjustStockRequest
{
    public int ProductVariantId { get; set; }
    public int Location { get; set; }
    public decimal QuantityChange { get; set; }
    public string? Notes { get; set; }
}
