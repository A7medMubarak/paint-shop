namespace PaintShop.Application.DTOs.Sales;

public class SaleDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Subtotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SaleItemDto> Items { get; set; } = [];
}
