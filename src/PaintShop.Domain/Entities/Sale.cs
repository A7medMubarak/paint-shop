using PaintShop.Domain.Enums;

namespace PaintShop.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int CustomerId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public User Employee { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = [];
}
