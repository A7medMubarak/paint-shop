namespace PaintShop.Application.DTOs.Sales;

public class SaleSummaryDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}
