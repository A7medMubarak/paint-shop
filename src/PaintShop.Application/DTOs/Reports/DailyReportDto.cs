namespace PaintShop.Application.DTOs.Reports;

public class DailyReportDto
{
    public DateOnly Date { get; set; }
    public int TotalSalesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscountsGiven { get; set; }
    public int CancelledSalesCount { get; set; }
    public List<EmployeeSalesDto> SalesByEmployee { get; set; } = [];
    public List<TopProductDto> TopVariants { get; set; } = [];
}

public class EmployeeSalesDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal DiscountsGiven { get; set; }
}

public class TopProductDto
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
