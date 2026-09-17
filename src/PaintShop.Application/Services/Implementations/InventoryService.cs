using Microsoft.EntityFrameworkCore;
using PaintShop.Application.Common;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Inventory;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Entities;
using PaintShop.Domain.Enums;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _context;
    private readonly InventoryOptions _options;

    public InventoryService(IApplicationDbContext context, InventoryOptions options)
    {
        _context = context;
        _options = options;
    }

    public async Task<List<InventoryItemDto>> GetAllAsync(int? location = null)
    {
        var query = _context.InventoryItems
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .AsQueryable();

        if (location.HasValue)
            query = query.Where(i => i.Location == (InventoryLocation)location.Value);

        var items = await query.ToListAsync();

        return items
            .GroupBy(i => i.ProductVariantId)
            .Select(g =>
            {
                var variant = g.First().Variant;
                var shopStock = g.Where(i => i.Location == InventoryLocation.Shop).Sum(i => i.Quantity);
                var warehouseStock = g.Where(i => i.Location == InventoryLocation.Warehouse).Sum(i => i.Quantity);
                return new InventoryItemDto
                {
                    VariantId = g.Key,
                    ProductName = variant.Product.Name,
                    BaseType = variant.BaseType?.ToString(),
                    SizeValue = variant.SizeValue,
                    SizeUnit = variant.SizeUnit,
                    ShopStock = shopStock,
                    WarehouseStock = warehouseStock,
                    IsLowStock = (variant.LowStockThreshold.HasValue && shopStock < variant.LowStockThreshold.Value)
                        || shopStock < _options.DefaultLowStockThreshold
                };
            })
            .ToList();
    }

    public async Task<PagedResult<InventoryItemDto>> GetFilteredAsync(InventoryFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.InventoryItems
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .AsQueryable();

        if (filter.Location.HasValue)
            query = query.Where(i => i.Location == (InventoryLocation)filter.Location.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(i => i.Variant.Product.Name.Contains(filter.Search));

        var items = await query.ToListAsync();

        var grouped = items
            .GroupBy(i => i.ProductVariantId)
            .Select(g =>
            {
                var variant = g.First().Variant;
                var shopStock = g.Where(i => i.Location == InventoryLocation.Shop).Sum(i => i.Quantity);
                var warehouseStock = g.Where(i => i.Location == InventoryLocation.Warehouse).Sum(i => i.Quantity);
                return new InventoryItemDto
                {
                    VariantId = g.Key,
                    ProductName = variant.Product.Name,
                    BaseType = variant.BaseType?.ToString(),
                    SizeValue = variant.SizeValue,
                    SizeUnit = variant.SizeUnit,
                    ShopStock = shopStock,
                    WarehouseStock = warehouseStock,
                    IsLowStock = (variant.LowStockThreshold.HasValue && shopStock < variant.LowStockThreshold.Value)
                        || shopStock < _options.DefaultLowStockThreshold
                };
            })
            .OrderBy(i => i.ProductName)
            .ToList();

        var totalCount = grouped.Count;
        var paged = grouped
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new PagedResult<InventoryItemDto>
        {
            Items = paged,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<InventoryItemDto>> GetLowStockAsync()
    {
        var shopItems = await _context.InventoryItems
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .Where(i => i.Location == InventoryLocation.Shop)
            .ToListAsync();

        var lowStockItems = shopItems
            .Where(i => (i.Variant.LowStockThreshold.HasValue && i.Quantity < i.Variant.LowStockThreshold.Value)
                || i.Quantity < _options.DefaultLowStockThreshold)
            .ToList();

        var variantIds = lowStockItems.Select(i => i.ProductVariantId).ToList();

        var warehouseStocks = await _context.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId) && i.Location == InventoryLocation.Warehouse)
            .ToDictionaryAsync(i => i.ProductVariantId, i => i.Quantity);

        return lowStockItems.Select(i => new InventoryItemDto
        {
            VariantId = i.ProductVariantId,
            ProductName = i.Variant.Product.Name,
            BaseType = i.Variant.BaseType?.ToString(),
            SizeValue = i.Variant.SizeValue,
            SizeUnit = i.Variant.SizeUnit,
            ShopStock = i.Quantity,
            WarehouseStock = warehouseStocks.GetValueOrDefault(i.ProductVariantId),
            IsLowStock = true
        }).ToList();
    }

    public async Task AddStockAsync(AddStockRequest request, int userId)
    {
        var location = (InventoryLocation)request.Location;
        var inventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductVariantId == request.ProductVariantId && i.Location == location)
            ?? throw new KeyNotFoundException("Inventory item not found for this variant and location");

        inventory.Quantity += request.Quantity;

        _context.StockMovements.Add(new StockMovement
        {
            ProductVariantId = request.ProductVariantId,
            Location = location,
            QuantityChange = request.Quantity,
            Reason = StockMovementReason.ManualAddition,
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();
    }

    public async Task AdjustStockAsync(AdjustStockRequest request, int userId)
    {
        var location = (InventoryLocation)request.Location;
        var inventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductVariantId == request.ProductVariantId && i.Location == location)
            ?? throw new KeyNotFoundException("Inventory item not found for this variant and location");

        inventory.Quantity += request.QuantityChange;

        _context.StockMovements.Add(new StockMovement
        {
            ProductVariantId = request.ProductVariantId,
            Location = location,
            QuantityChange = request.QuantityChange,
            Reason = StockMovementReason.Adjustment,
            Notes = request.Notes,
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now,
            StockTransfer = null
        });

        await _context.SaveChangesAsync();
    }

    public async Task TransferStockAsync(TransferStockRequest request, int userId)
    {
        var fromLocation = (InventoryLocation)request.FromLocation;
        var toLocation = (InventoryLocation)request.ToLocation;
        var quantity = request.Quantity;

        var fromInventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductVariantId == request.ProductVariantId && i.Location == fromLocation)
            ?? throw new KeyNotFoundException("Source inventory item not found");

        if (fromInventory.Quantity < quantity)
            throw new InvalidOperationException("Insufficient stock at source location");

        var toInventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductVariantId == request.ProductVariantId && i.Location == toLocation)
            ?? throw new KeyNotFoundException("Destination inventory item not found");

        fromInventory.Quantity -= quantity;
        toInventory.Quantity += quantity;

        var transfer = new StockTransfer
        {
            FromLocation = fromLocation,
            ToLocation = toLocation,
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now,
            Notes = request.Notes
        };

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();

        _context.StockMovements.Add(new StockMovement
        {
            ProductVariantId = request.ProductVariantId,
            Location = fromLocation,
            QuantityChange = -quantity,
            Reason = StockMovementReason.Transfer,
            StockTransferId = transfer.Id,
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now
        });

        _context.StockMovements.Add(new StockMovement
        {
            ProductVariantId = request.ProductVariantId,
            Location = toLocation,
            QuantityChange = quantity,
            Reason = StockMovementReason.Transfer,
            StockTransferId = transfer.Id,
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<StockMovementDto>> GetMovementsAsync(int variantId)
    {
        var movements = await _context.StockMovements
            .Include(m => m.CreatedBy)
            .Where(m => m.ProductVariantId == variantId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return movements.Select(m => new StockMovementDto
        {
            Id = m.Id,
            Location = m.Location.ToString(),
            QuantityChange = m.QuantityChange,
            Reason = m.Reason.ToString(),
            CreatedBy = m.CreatedBy?.Username ?? "—",
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}
