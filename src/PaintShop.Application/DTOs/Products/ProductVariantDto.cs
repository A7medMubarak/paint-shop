namespace PaintShop.Application.DTOs.Products;

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = string.Empty;
    public decimal? SellingPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
    public decimal ShopStock { get; set; }
    public decimal WarehouseStock { get; set; }
    public bool IsLowStock { get; set; }
}
