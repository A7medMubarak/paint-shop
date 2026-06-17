namespace PaintShop.Application.DTOs.Products;

public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int VariantCount { get; set; }
}
