using PaintShop.Domain.Enums;

namespace PaintShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductCategory ProductCategory { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
