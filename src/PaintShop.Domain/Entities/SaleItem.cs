namespace PaintShop.Domain.Entities;

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ColorCode { get; set; }
    public Sale Sale { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
