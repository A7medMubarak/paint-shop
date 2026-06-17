namespace PaintShop.Application.DTOs.Inventory;

public class AddStockRequest
{
    public int ProductVariantId { get; set; }
    public int Location { get; set; }
    public decimal Quantity { get; set; }
}
