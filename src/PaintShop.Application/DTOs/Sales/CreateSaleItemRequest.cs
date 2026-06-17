namespace PaintShop.Application.DTOs.Sales;

public class CreateSaleItemRequest
{
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ColorCode { get; set; }
}
