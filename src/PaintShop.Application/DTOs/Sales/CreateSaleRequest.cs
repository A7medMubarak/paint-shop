namespace PaintShop.Application.DTOs.Sales;

public class CreateSaleRequest
{
    public int CustomerId { get; set; } = 1;
    public decimal DiscountAmount { get; set; }
    public bool ForceNegativeInventory { get; set; }
    public List<CreateSaleItemRequest> Items { get; set; } = [];
}
