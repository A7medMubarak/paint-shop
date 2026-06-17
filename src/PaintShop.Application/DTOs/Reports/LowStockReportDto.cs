namespace PaintShop.Application.DTOs.Reports;

public class LowStockReportDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BaseType { get; set; }
    public decimal SizeValue { get; set; }
    public string SizeUnit { get; set; } = string.Empty;
    public decimal ShopStock { get; set; }
    public decimal Threshold { get; set; }
}
