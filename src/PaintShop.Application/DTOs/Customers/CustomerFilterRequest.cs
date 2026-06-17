namespace PaintShop.Application.DTOs.Customers;

public class CustomerFilterRequest
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
