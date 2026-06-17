namespace PaintShop.Application.DTOs.Users;

public class CreateEmployeeRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
