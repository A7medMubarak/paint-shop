using PaintShop.Domain.Enums;

namespace PaintShop.Domain.Entities;

public class InventoryItem
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public InventoryLocation Location { get; set; }
    public decimal Quantity { get; set; }
    public ProductVariant Variant { get; set; } = null!;
}
