namespace PaintShop.Application.DTOs.Products;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public int ProductCategory { get; set; }
}
