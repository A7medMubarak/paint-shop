using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintShop.Application.DTOs.Reports;
using PaintShop.Application.Services.Interfaces;

namespace PaintShop.API.Controllers;

[ApiController]
[Route("reports")]
[Authorize(Roles = "Owner")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<DailyReportDto>> Daily([FromQuery] DateTime date)
    {
        return Ok(await _reportService.GetDailyReportAsync(date));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockReportDto>>> LowStock()
    {
        return Ok(await _reportService.GetLowStockReportAsync());
    }

    [HttpGet("top-selling")]
    public async Task<ActionResult<List<TopSellingDto>>> TopSelling([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return Ok(await _reportService.GetTopSellingAsync(from, to));
    }

    [HttpGet("period")]
    public async Task<ActionResult<PeriodReportDto>> Period([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return Ok(await _reportService.GetPeriodReportAsync(from, to));
    }

    [HttpGet("inventory-valuation")]
    public async Task<ActionResult<List<InventoryValuationDto>>> InventoryValuation()
    {
        return Ok(await _reportService.GetInventoryValuationAsync());
    }
}
