using PaintShop.Application.Common;
using PaintShop.Application.DTOs.Inventory;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class InventoryServiceTests
{
    private readonly InventoryOptions _options = new() { DefaultLowStockThreshold = 5 };

    private (List<Product> Products, List<ProductVariant> Variants, List<InventoryItem> Items) SeedData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Paint B", ProductCategory = ProductCategory.GlcDecore, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = new List<ProductVariant>
        {
            new() { Id = 1, ProductId = 1, SizeValue = 1, SizeUnit = "L", LowStockThreshold = 5, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, ProductId = 2, SizeValue = 3, SizeUnit = "L", LowStockThreshold = 3, IsActive = true, CreatedAt = DateTime.Now }
        };
        var inventory = new List<InventoryItem>
        {
            new() { Id = 1, ProductVariantId = 1, Location = InventoryLocation.Shop, Quantity = 10 },
            new() { Id = 2, ProductVariantId = 1, Location = InventoryLocation.Warehouse, Quantity = 20 },
            new() { Id = 3, ProductVariantId = 2, Location = InventoryLocation.Shop, Quantity = 2 },
            new() { Id = 4, ProductVariantId = 2, Location = InventoryLocation.Warehouse, Quantity = 15 }
        };
        return (products, variants, inventory);
    }

    [Fact]
    public async Task GetFilteredAsync_WithLocationFilter_ReturnsFilteredItems()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        var result = await service.GetFilteredAsync(new InventoryFilterRequest { Location = 0, Page = 1, PageSize = 20 });

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetFilteredAsync_WithSearch_FiltersByProductName()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        var result = await service.GetFilteredAsync(new InventoryFilterRequest { Search = "Paint B", Page = 1, PageSize = 20 });

        result.Items.Should().ContainSingle();
        result.Items[0].ProductName.Should().Be("Paint B");
    }

    [Fact]
    public async Task AddStockAsync_WithValidRequest_AddsQuantity()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        await service.AddStockAsync(new AddStockRequest { ProductVariantId = 1, Location = 0, Quantity = 5 }, 1);

        var item = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Shop);
        item.Quantity.Should().Be(15);
    }

    [Fact]
    public async Task AdjustStockAsync_WithValidRequest_AdjustsQuantity()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        await service.AdjustStockAsync(new AdjustStockRequest { ProductVariantId = 1, Location = 0, QuantityChange = -3 }, 1);

        var item = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Shop);
        item.Quantity.Should().Be(7);
    }

    [Fact]
    public async Task TransferStockAsync_WithValidRequest_TransfersQuantity()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        await service.TransferStockAsync(new TransferStockRequest { ProductVariantId = 1, Quantity = 5, FromLocation = 0, ToLocation = 1 }, 1);

        var fromItem = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Shop);
        var toItem = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Warehouse);
        fromItem.Quantity.Should().Be(5);
        toItem.Quantity.Should().Be(25);
    }

    [Fact]
    public async Task TransferStockAsync_InsufficientStock_ThrowsException()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        var act = () => service.TransferStockAsync(new TransferStockRequest { ProductVariantId = 1, Quantity = 100, FromLocation = 0, ToLocation = 1 }, 1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsItemsBelowThreshold()
    {
        var (products, variants, inventory) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory);
        var service = new InventoryService(ctx, _options);

        var result = await service.GetLowStockAsync();

        result.Should().ContainSingle();
        result[0].ProductName.Should().Be("Paint B");
    }
}
