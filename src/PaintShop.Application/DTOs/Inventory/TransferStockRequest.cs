namespace PaintShop.Application.DTOs.Inventory;

public class TransferStockRequest
{
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public int FromLocation { get; set; }
    public int ToLocation { get; set; }
    public string? Notes { get; set; }
}
