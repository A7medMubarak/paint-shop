namespace PaintShop.Application.DTOs.Customers;

public class CustomerDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Sales.SaleSummaryDto> RecentSales { get; set; } = [];
}
