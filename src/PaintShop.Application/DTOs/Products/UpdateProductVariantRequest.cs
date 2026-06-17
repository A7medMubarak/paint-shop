namespace PaintShop.Application.DTOs.Products;

public class UpdateProductVariantRequest
{
    public decimal? SellingPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public bool? IsActive { get; set; }
}
