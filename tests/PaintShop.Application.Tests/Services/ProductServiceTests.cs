using PaintShop.Application.Common;
using PaintShop.Application.DTOs.Products;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly InventoryOptions _options = new() { DefaultLowStockThreshold = 5 };

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsAllProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Paint B", ProductCategory = ProductCategory.GlcDecore, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = new List<ProductVariant>
        {
            new() { Id = 1, ProductId = 1, SizeValue = 1, SizeUnit = "L", IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, ProductId = 2, SizeValue = 5, SizeUnit = "L", IsActive = true, CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(products: products, variants: variants);
        var service = new ProductService(ctx, _options);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(products: products);
        var service = new ProductService(ctx, _options);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("Paint A");
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesProduct()
    {
        var ctx = MockDbContext.Create();
        var service = new ProductService(ctx, _options);
        var request = new CreateProductRequest { Name = "New Paint", ProductCategory = 0 };

        var result = await service.CreateAsync(request);

        result.Name.Should().Be("New Paint");
        result.ProductCategory.Should().Be("GlcPlastic");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesProduct()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(products: products);
        var service = new ProductService(ctx, _options);

        var result = await service.UpdateAsync(1, new CreateProductRequest { Name = "Updated", ProductCategory = 1 });

        result.Name.Should().Be("Updated");
        result.ProductCategory.Should().Be("GlcDecore");
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenProductExists_TogglesIsActive()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(products: products);
        var service = new ProductService(ctx, _options);

        await service.ToggleActiveAsync(1);

        var updated = await ctx.Products.FindAsync(1);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateVariantAsync_WithValidData_CreatesVariantAndInventory()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var ctx = MockDbContext.Create(products: products);
        var service = new ProductService(ctx, _options);

        var result = await service.CreateVariantAsync(1, new CreateProductVariantRequest
        {
            BaseType = 0,
            SizeValue = 2,
            SizeUnit = "L",
            SellingPrice = 100,
            CostPrice = 50
        });

        result.BaseType.Should().Be("BaseA");
        result.SizeValue.Should().Be(2);

        var inventory = await ctx.InventoryItems.ToListAsync();
        inventory.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilteredAsync_WithPagination_ReturnsPagedResults()
    {
        var products = Enumerable.Range(1, 25).Select(i => new Product
        {
            Id = i,
            Name = $"Paint {i}",
            ProductCategory = ProductCategory.GlcPlastic,
            IsActive = true,
            CreatedAt = DateTime.Now
        }).ToList();
        var variants = products.Select(p => new ProductVariant
        {
            Id = p.Id,
            ProductId = p.Id,
            SizeValue = 1,
            SizeUnit = "L",
            IsActive = true,
            CreatedAt = DateTime.Now
        }).ToList();
        var ctx = MockDbContext.Create(products: products, variants: variants);
        var service = new ProductService(ctx, _options);

        var result = await service.GetFilteredAsync(new ProductFilterRequest { Page = 1, PageSize = 10 });

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetFilteredAsync_WithSearch_FiltersByName()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Red Paint", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Blue Paint", ProductCategory = ProductCategory.GlcDecore, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = products.Select(p => new ProductVariant
        {
            Id = p.Id,
            ProductId = p.Id,
            SizeValue = 1,
            SizeUnit = "L",
            IsActive = true,
            CreatedAt = DateTime.Now
        }).ToList();
        var ctx = MockDbContext.Create(products: products, variants: variants);
        var service = new ProductService(ctx, _options);

        var result = await service.GetFilteredAsync(new ProductFilterRequest { Search = "Blue", Page = 1, PageSize = 20 });

        result.Items.Should().ContainSingle();
        result.Items[0].Name.Should().Be("Blue Paint");
    }

    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsMatchingProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Red Paint", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Blue Paint", ProductCategory = ProductCategory.GlcDecore, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = products.Select(p => new ProductVariant
        {
            Id = p.Id,
            ProductId = p.Id,
            SizeValue = 1,
            SizeUnit = "L",
            IsActive = true,
            CreatedAt = DateTime.Now
        }).ToList();
        var ctx = MockDbContext.Create(products: products, variants: variants);
        var service = new ProductService(ctx, _options);

        var result = await service.SearchAsync("Red");

        result.Should().ContainSingle();
    }
}
