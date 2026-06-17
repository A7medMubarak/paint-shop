namespace PaintShop.Application.DTOs.Customers;

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
