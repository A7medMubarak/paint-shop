namespace PaintShop.Application.DTOs.Reports;

public class TopSellingDto
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
