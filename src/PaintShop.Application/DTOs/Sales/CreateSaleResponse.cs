namespace PaintShop.Application.DTOs.Sales;

public class CreateSaleResponse
{
    public SaleDto Sale { get; set; } = null!;
    public bool InventoryWarning { get; set; }
    public List<int> WarningVariantIds { get; set; } = [];
}
