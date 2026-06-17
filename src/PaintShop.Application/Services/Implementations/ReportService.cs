using Microsoft.EntityFrameworkCore;
using PaintShop.Application.DTOs.Reports;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Enums;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    public ReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var sales = await _context.Sales
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(si => si.Variant)
                    .ThenInclude(v => v.Product)
            .Where(s => s.CreatedAt >= dayStart && s.CreatedAt < dayEnd)
            .ToListAsync();

        var activeSales = sales.Where(s => s.Status == SaleStatus.Active).ToList();
        var cancelledSales = sales.Where(s => s.Status == SaleStatus.Cancelled).ToList();

        var byEmployee = activeSales
            .GroupBy(s => new { s.EmployeeId, s.Employee.Username })
            .Select(g => new EmployeeSalesDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = g.Key.Username,
                SalesCount = g.Count(),
                Revenue = g.Sum(s => s.TotalAmount),
                DiscountsGiven = g.Sum(s => s.DiscountAmount)
            })
            .ToList();

        var topVariants = activeSales
            .SelectMany(s => s.Items)
            .GroupBy(si => new { si.ProductVariantId, si.Variant.Product.Name, si.Variant.BaseType, si.Variant.SizeValue, si.Variant.SizeUnit })
            .Select(g => new TopProductDto
            {
                VariantId = g.Key.ProductVariantId,
                VariantName = $"{g.Key.Name} ({g.Key.BaseType?.ToString() ?? "-"}, {g.Key.SizeValue} {g.Key.SizeUnit})",
                QuantitySold = g.Sum(si => si.Quantity),
                Revenue = g.Sum(si => si.UnitPrice * si.Quantity)
            })
            .OrderByDescending(t => t.QuantitySold)
            .Take(10)
            .ToList();

        return new DailyReportDto
        {
            Date = DateOnly.FromDateTime(date),
            TotalSalesCount = activeSales.Count,
            TotalRevenue = activeSales.Sum(s => s.TotalAmount),
            TotalDiscountsGiven = activeSales.Sum(s => s.DiscountAmount),
            CancelledSalesCount = cancelledSales.Count,
            SalesByEmployee = byEmployee,
            TopVariants = topVariants
        };
    }

    public async Task<List<LowStockReportDto>> GetLowStockReportAsync()
    {
        var shopItems = await _context.InventoryItems
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .Where(i => i.Location == InventoryLocation.Shop)
            .ToListAsync();

        return shopItems
            .Where(i => i.Variant.LowStockThreshold.HasValue && i.Quantity < i.Variant.LowStockThreshold.Value)
            .Select(i => new LowStockReportDto
            {
                VariantId = i.ProductVariantId,
                ProductName = i.Variant.Product.Name,
                BaseType = i.Variant.BaseType?.ToString() ?? "N/A",
                SizeValue = i.Variant.SizeValue,
                SizeUnit = i.Variant.SizeUnit,
                ShopStock = i.Quantity,
                Threshold = i.Variant.LowStockThreshold!.Value
            })
            .OrderBy(r => r.ShopStock)
            .ToList();
    }

    public async Task<List<TopSellingDto>> GetTopSellingAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        var items = await _context.SaleItems
            .Include(si => si.Variant)
                .ThenInclude(v => v.Product)
            .Include(si => si.Sale)
            .Where(si => si.Sale.CreatedAt >= from && si.Sale.CreatedAt < toEnd && si.Sale.Status == SaleStatus.Active)
            .ToListAsync();

        return items
            .GroupBy(si => new { si.ProductVariantId, si.Variant.Product.Name, si.Variant.BaseType, si.Variant.SizeValue, si.Variant.SizeUnit })
            .Select(g => new TopSellingDto
            {
                VariantId = g.Key.ProductVariantId,
                VariantName = $"{g.Key.Name} ({g.Key.BaseType?.ToString() ?? "-"}, {g.Key.SizeValue} {g.Key.SizeUnit})",
                QuantitySold = g.Sum(si => si.Quantity),
                Revenue = g.Sum(si => si.UnitPrice * si.Quantity)
            })
            .OrderByDescending(t => t.QuantitySold)
            .Take(10)
            .ToList();
    }

    public async Task<PeriodReportDto> GetPeriodReportAsync(DateTime from, DateTime to)
    {
        var toEnd = to.Date.AddDays(1);

        var sales = await _context.Sales
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(si => si.Variant)
                    .ThenInclude(v => v.Product)
            .Where(s => s.CreatedAt >= from && s.CreatedAt < toEnd)
            .ToListAsync();

        var activeSales = sales.Where(s => s.Status == SaleStatus.Active).ToList();
        var cancelledSales = sales.Where(s => s.Status == SaleStatus.Cancelled).ToList();

        var byEmployee = activeSales
            .GroupBy(s => new { s.EmployeeId, s.Employee.Username })
            .Select(g => new EmployeeSalesDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = g.Key.Username,
                SalesCount = g.Count(),
                Revenue = g.Sum(s => s.TotalAmount),
                DiscountsGiven = g.Sum(s => s.DiscountAmount)
            })
            .ToList();

        var topVariants = activeSales
            .SelectMany(s => s.Items)
            .GroupBy(si => new { si.ProductVariantId, si.Variant.Product.Name, si.Variant.BaseType, si.Variant.SizeValue, si.Variant.SizeUnit })
            .Select(g => new TopProductDto
            {
                VariantId = g.Key.ProductVariantId,
                VariantName = $"{g.Key.Name} ({g.Key.BaseType?.ToString() ?? "-"}, {g.Key.SizeValue} {g.Key.SizeUnit})",
                QuantitySold = g.Sum(si => si.Quantity),
                Revenue = g.Sum(si => si.UnitPrice * si.Quantity)
            })
            .OrderByDescending(t => t.QuantitySold)
            .Take(10)
            .ToList();

        return new PeriodReportDto
        {
            From = DateOnly.FromDateTime(from),
            To = DateOnly.FromDateTime(to),
            TotalSalesCount = activeSales.Count,
            TotalRevenue = activeSales.Sum(s => s.TotalAmount),
            TotalDiscountsGiven = activeSales.Sum(s => s.DiscountAmount),
            CancelledSalesCount = cancelledSales.Count,
            SalesByEmployee = byEmployee,
            TopVariants = topVariants
        };
    }

    public async Task<List<InventoryValuationDto>> GetInventoryValuationAsync()
    {
        var locations = new[] { "Shop", "Warehouse" };
        var result = new List<InventoryValuationDto>();

        foreach (var location in locations)
        {
            var loc = location == "Shop" ? InventoryLocation.Shop : InventoryLocation.Warehouse;

            var items = await _context.InventoryItems
                .Include(i => i.Variant)
                    .ThenInclude(v => v.Product)
                .Where(i => i.Location == loc)
                .ToListAsync();

            var itemDtos = items.Select(i =>
            {
                var unitValue = i.Variant.CostPrice ?? i.Variant.SellingPrice ?? 0;
                return new InventoryItemValuationDto
                {
                    VariantId = i.ProductVariantId,
                    ProductName = i.Variant.Product.Name,
                    BaseType = i.Variant.BaseType?.ToString(),
                    SizeValue = i.Variant.SizeValue,
                    SizeUnit = i.Variant.SizeUnit,
                    Quantity = i.Quantity,
                    CostPrice = i.Variant.CostPrice,
                    SellingPrice = i.Variant.SellingPrice,
                    ItemValue = i.Quantity * unitValue
                };
            }).OrderByDescending(d => d.ItemValue).ToList();

            result.Add(new InventoryValuationDto
            {
                Location = location,
                TotalItems = items.Count,
                TotalValue = itemDtos.Sum(d => d.ItemValue),
                Items = itemDtos
            });
        }

        return result;
    }
}
