using Microsoft.EntityFrameworkCore;
using PaintShop.Application.Common;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Sales;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Entities;
using PaintShop.Domain.Enums;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class SaleService : ISaleService
{
    private readonly IApplicationDbContext _context;

    public SaleService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateSaleResponse> CreateSaleAsync(CreateSaleRequest request, int employeeId)
    {
        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync();

        var missingIds = variantIds.Except(variants.Select(v => v.Id)).ToList();
        if (missingIds.Count != 0)
            throw new KeyNotFoundException($"Variants not found: {string.Join(", ", missingIds)}");

        var inactiveVariants = variants.Where(v => !v.IsActive).Select(v => v.Id).ToList();
        if (inactiveVariants.Count != 0)
            throw new InvalidOperationException($"Inactive variants: {string.Join(", ", inactiveVariants)}");

        var aggregatedItems = request.Items
            .GroupBy(i => i.ProductVariantId)
            .Select(g => new
            {
                ProductVariantId = g.Key,
                Quantity = g.Sum(i => i.Quantity),
                UnitPrice = g.First().UnitPrice,
                ColorCode = g.First().ColorCode,
                Variant = variants.First(v => v.Id == g.Key)
            })
            .ToList();

        var shopItems = await _context.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId) && i.Location == InventoryLocation.Shop)
            .ToDictionaryAsync(i => i.ProductVariantId);

        var warningIds = new List<int>();
        foreach (var item in aggregatedItems)
        {
            if (!shopItems.TryGetValue(item.ProductVariantId, out var shopItem))
                throw new KeyNotFoundException($"Shop inventory not found for variant {item.ProductVariantId}");

            if (shopItem.Quantity < item.Quantity && !request.ForceNegativeInventory)
                throw new InvalidOperationException(
                    $"Insufficient stock for variant {item.ProductVariantId}: " +
                    $"requested {item.Quantity}, available {shopItem.Quantity}");

            if (shopItem.Quantity < item.Quantity)
                warningIds.Add(item.ProductVariantId);
        }

        var subtotal = SaleCalculator.Subtotal(aggregatedItems.Select(i => (i.UnitPrice, i.Quantity)));
        var totalAmount = SaleCalculator.TotalAmount(subtotal, request.DiscountAmount);

        var sale = new Sale
        {
            EmployeeId = employeeId,
            CustomerId = request.CustomerId,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = totalAmount,
            Status = SaleStatus.Active,
            CreatedAt = DateTime.Now
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        foreach (var item in aggregatedItems)
        {
            var variant = item.Variant;
            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                OriginalPrice = variant.SellingPrice,
                UnitPrice = item.UnitPrice,
                ColorCode = item.ColorCode
            });

            var shopItem = shopItems[item.ProductVariantId];
            shopItem.Quantity -= item.Quantity;

            _context.StockMovements.Add(new StockMovement
            {
                ProductVariantId = item.ProductVariantId,
                Location = InventoryLocation.Shop,
                QuantityChange = -item.Quantity,
                Reason = StockMovementReason.Sale,
                ReferenceSaleId = sale.Id,
                CreatedByUserId = employeeId,
                CreatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();

        var employeeName = await _context.Users
            .Where(u => u.Id == employeeId).Select(u => u.Username).FirstAsync();
        var customerName = await _context.Customers
            .Where(c => c.Id == request.CustomerId).Select(c => c.Name).FirstAsync();

        var variantDetails = await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        var dto = new SaleDto
        {
            Id = sale.Id,
            EmployeeName = employeeName,
            CustomerName = customerName,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            Subtotal = subtotal,
            Status = SaleStatus.Active.ToString(),
            CreatedAt = sale.CreatedAt,
            Items = sale.Items.Select(i =>
            {
                var v = variantDetails.GetValueOrDefault(i.ProductVariantId);
                return new SaleItemDto
                {
                    Id = i.Id,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = v?.Product?.Name ?? "",
                    BaseType = v?.BaseType?.ToString(),
                    SizeValue = v?.SizeValue ?? 0,
                    SizeUnit = v?.SizeUnit ?? "",
                    Quantity = i.Quantity,
                    OriginalPrice = i.OriginalPrice,
                    UnitPrice = i.UnitPrice,
                    ColorCode = i.ColorCode
                };
            }).ToList()
        };

        return new CreateSaleResponse
        {
            Sale = dto,
            InventoryWarning = warningIds.Count != 0,
            WarningVariantIds = warningIds
        };
    }

    public async Task<SaleDto> GetByIdAsync(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Employee)
            .Include(s => s.Customer)
            .Include(s => s.Items)
                .ThenInclude(si => si.Variant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Sale not found");

        return MapToSaleDto(sale);
    }

    public async Task<PagedResult<SaleSummaryDto>> GetFilteredAsync(SaleFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.Sales
            .Include(s => s.Employee)
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .AsQueryable();

        if (filter.Date.HasValue)
            query = query.Where(s => s.CreatedAt.Date == filter.Date.Value.Date);

        if (filter.Status.HasValue)
            query = query.Where(s => s.Status == (SaleStatus)filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(s => s.Customer.Name.Contains(filter.Search) || s.Employee.Username.Contains(filter.Search));

        var totalCount = await query.CountAsync();

        var sales = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<SaleSummaryDto>
        {
            Items = sales.Select(s => new SaleSummaryDto
            {
                Id = s.Id,
                EmployeeName = s.Employee.Username,
                CustomerName = s.Customer.Name,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt,
                ItemCount = s.Items.Count
            }).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task CancelSaleAsync(int saleId, int userId)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId)
            ?? throw new KeyNotFoundException("Sale not found");

        if (sale.Status == SaleStatus.Cancelled)
            throw new InvalidOperationException("Sale is already cancelled");

        sale.Status = SaleStatus.Cancelled;

        foreach (var item in sale.Items)
        {
            var inventory = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.ProductVariantId == item.ProductVariantId && i.Location == InventoryLocation.Shop);

            if (inventory != null)
                inventory.Quantity += item.Quantity;

            _context.StockMovements.Add(new StockMovement
            {
                ProductVariantId = item.ProductVariantId,
                Location = InventoryLocation.Shop,
                QuantityChange = item.Quantity,
                Reason = StockMovementReason.Cancellation,
                ReferenceSaleId = saleId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }

    private static SaleDto MapToSaleDto(Sale sale)
    {
        var subtotal = sale.Items.Sum(i => i.UnitPrice * i.Quantity);

        return new SaleDto
        {
            Id = sale.Id,
            EmployeeName = sale.Employee?.Username ?? "",
            CustomerName = sale.Customer?.Name ?? "",
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            Subtotal = subtotal,
            Status = sale.Status.ToString(),
            CreatedAt = sale.CreatedAt,
            Items = sale.Items.Select(i => new SaleItemDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.Variant?.Product?.Name ?? "",
                BaseType = i.Variant?.BaseType?.ToString(),
                SizeValue = i.Variant?.SizeValue ?? 0,
                SizeUnit = i.Variant?.SizeUnit ?? "",
                Quantity = i.Quantity,
                OriginalPrice = i.OriginalPrice,
                UnitPrice = i.UnitPrice,
                ColorCode = i.ColorCode
            }).ToList()
        };
    }
}
