namespace PaintShop.Application.DTOs.Reports;

public class InventoryValuationDto
{
    public string Location { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalValue { get; set; }
    public List<InventoryItemValuationDto> Items { get; set; } = [];
}

public class InventoryItemValuationDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = "L";
    public decimal Quantity { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal ItemValue { get; set; }
}
