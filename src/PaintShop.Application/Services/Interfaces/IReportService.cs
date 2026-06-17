using PaintShop.Application.DTOs.Reports;

namespace PaintShop.Application.Services.Interfaces;

public interface IReportService
{
    Task<DailyReportDto> GetDailyReportAsync(DateTime date);
    Task<List<LowStockReportDto>> GetLowStockReportAsync();
    Task<List<TopSellingDto>> GetTopSellingAsync(DateTime from, DateTime to);
    Task<PeriodReportDto> GetPeriodReportAsync(DateTime from, DateTime to);
    Task<List<InventoryValuationDto>> GetInventoryValuationAsync();
}
