namespace PaintShop.Application.DTOs.Inventory;

public class StockMovementDto
{
    public int Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
