namespace PaintShop.Application.DTOs.Reports;

public class PeriodReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalSalesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscountsGiven { get; set; }
    public int CancelledSalesCount { get; set; }
    public List<EmployeeSalesDto> SalesByEmployee { get; set; } = [];
    public List<TopProductDto> TopVariants { get; set; } = [];
}
