namespace PaintShop.Application.DTOs.Sales;

public class SaleItemDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ColorCode { get; set; }
}
