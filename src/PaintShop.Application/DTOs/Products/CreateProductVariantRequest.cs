namespace PaintShop.Application.DTOs.Products;

public class CreateProductVariantRequest
{
    public int? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = "L";
    public decimal? SellingPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? LowStockThreshold { get; set; }
}
