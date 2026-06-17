using PaintShop.Domain.Enums;

namespace PaintShop.Domain.Entities;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public BaseType? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = "L";
    public decimal? SellingPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Product Product { get; set; } = null!;
}
