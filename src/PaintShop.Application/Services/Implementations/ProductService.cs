using Microsoft.EntityFrameworkCore;
using PaintShop.Application.Common;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Products;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Entities;
using PaintShop.Domain.Enums;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;
    private readonly InventoryOptions _options;

    public ProductService(IApplicationDbContext context, InventoryOptions options)
    {
        _context = context;
        _options = options;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _context.Products
            .Include(p => p.Variants)
            .ToListAsync();

        var variantIds = products.SelectMany(p => p.Variants).Select(v => v.Id).ToList();
        var inventoryItems = await GetInventoryLookupAsync(variantIds);

        return products.Select(p => MapToDto(p, inventoryItems)).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Product not found");

        var variantIds = product.Variants.Select(v => v.Id).ToList();
        var inventoryItems = await GetInventoryLookupAsync(variantIds);

        return MapToDto(product, inventoryItems);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            ProductCategory = (ProductCategory)request.ProductCategory,
            CreatedAt = DateTime.Now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDto(product, []);
    }

    public async Task<ProductDto> UpdateAsync(int id, CreateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Product not found");

        product.Name = request.Name;
        product.ProductCategory = (ProductCategory)request.ProductCategory;

        await _context.SaveChangesAsync();

        var variantIds = product.Variants.Select(v => v.Id).ToList();
        var inventoryItems = await GetInventoryLookupAsync(variantIds);

        return MapToDto(product, inventoryItems);
    }

    public async Task ToggleActiveAsync(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found");
        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<ProductDto>> GetFilteredAsync(ProductFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.Products
            .Include(p => p.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search));

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var variantIds = products.SelectMany(p => p.Variants).Select(v => v.Id).ToList();
        var inventoryItems = await GetInventoryLookupAsync(variantIds);

        return new PagedResult<ProductDto>
        {
            Items = products.Select(p => MapToDto(p, inventoryItems)).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<ProductDto>> SearchAsync(string query)
    {
        var products = await _context.Products
            .Include(p => p.Variants)
            .Where(p => p.Name.Contains(query))
            .ToListAsync();

        var variantIds = products.SelectMany(p => p.Variants).Select(v => v.Id).ToList();
        var inventoryItems = await GetInventoryLookupAsync(variantIds);

        return products.Select(p => MapToDto(p, inventoryItems)).ToList();
    }

    public async Task<ProductVariantDto> CreateVariantAsync(int productId, CreateProductVariantRequest request)
    {
        var product = await _context.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException("Product not found");

        var variant = new ProductVariant
        {
            ProductId = productId,
            BaseType = request.BaseType.HasValue ? (BaseType)request.BaseType.Value : null,
            SizeValue = request.SizeValue,
            SizeUnit = request.SizeUnit,
            SellingPrice = request.SellingPrice,
            CostPrice = request.CostPrice,
            LowStockThreshold = request.LowStockThreshold,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        _context.InventoryItems.Add(new InventoryItem
        {
            ProductVariantId = variant.Id,
            Location = InventoryLocation.Shop,
            Quantity = 0
        });
        _context.InventoryItems.Add(new InventoryItem
        {
            ProductVariantId = variant.Id,
            Location = InventoryLocation.Warehouse,
            Quantity = 0
        });

        await _context.SaveChangesAsync();

        return MapVariantToDto(variant, 0, 0);
    }

    public async Task<ProductVariantDto> UpdateVariantAsync(int id, UpdateProductVariantRequest request)
    {
        var variant = await _context.ProductVariants.FindAsync(id)
            ?? throw new KeyNotFoundException("Variant not found");

        if (request.SellingPrice.HasValue) variant.SellingPrice = request.SellingPrice;
        if (request.CostPrice.HasValue) variant.CostPrice = request.CostPrice;
        if (request.LowStockThreshold.HasValue) variant.LowStockThreshold = request.LowStockThreshold;
        if (request.IsActive.HasValue) variant.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        var shopStock = await _context.InventoryItems
            .Where(i => i.ProductVariantId == variant.Id && i.Location == InventoryLocation.Shop)
            .SumAsync(i => i.Quantity);
        var warehouseStock = await _context.InventoryItems
            .Where(i => i.ProductVariantId == variant.Id && i.Location == InventoryLocation.Warehouse)
            .SumAsync(i => i.Quantity);

        return MapVariantToDto(variant, shopStock, warehouseStock);
    }

    public async Task ToggleVariantActiveAsync(int id)
    {
        var variant = await _context.ProductVariants.FindAsync(id)
            ?? throw new KeyNotFoundException("Variant not found");

        variant.IsActive = !variant.IsActive;
        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<int, Dictionary<InventoryLocation, decimal>>> GetInventoryLookupAsync(List<int> variantIds)
    {
        if (variantIds.Count == 0) return [];

        var items = await _context.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .ToListAsync();

        return items
            .GroupBy(i => i.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(i => i.Location, i => i.Quantity));
    }

    private ProductDto MapToDto(Product product, Dictionary<int, Dictionary<InventoryLocation, decimal>> stockLookup)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            ProductCategory = product.ProductCategory.ToString(),
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            Variants = product.Variants.Select(v =>
            {
                var lookup = stockLookup.GetValueOrDefault(v.Id);
                var shopStock = lookup?.GetValueOrDefault(InventoryLocation.Shop) ?? 0;
                var warehouseStock = lookup?.GetValueOrDefault(InventoryLocation.Warehouse) ?? 0;
                return MapVariantToDto(v, shopStock, warehouseStock);
            }).ToList()
        };
    }

    private ProductVariantDto MapVariantToDto(ProductVariant variant, decimal shopStock, decimal warehouseStock)
    {
        return new ProductVariantDto
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            BaseType = variant.BaseType?.ToString(),
            SizeValue = variant.SizeValue,
            SizeUnit = variant.SizeUnit,
            SellingPrice = variant.SellingPrice,
            CostPrice = variant.CostPrice,
            LowStockThreshold = variant.LowStockThreshold,
            IsActive = variant.IsActive,
            ShopStock = shopStock,
            WarehouseStock = warehouseStock,
            IsLowStock = (variant.LowStockThreshold.HasValue && shopStock < variant.LowStockThreshold.Value)
                || shopStock < _options.DefaultLowStockThreshold
        };
    }
}
