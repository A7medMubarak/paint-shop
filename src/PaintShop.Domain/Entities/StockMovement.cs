using PaintShop.Domain.Enums;

namespace PaintShop.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public InventoryLocation Location { get; set; }
    public decimal QuantityChange { get; set; }
    public StockMovementReason Reason { get; set; }
    public string? Notes { get; set; }
    public int? ReferenceSaleId { get; set; }
    public int? StockTransferId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProductVariant Variant { get; set; } = null!;
    public Sale? Sale { get; set; }
    public StockTransfer? StockTransfer { get; set; }
    public User CreatedBy { get; set; } = null!;
}
