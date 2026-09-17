using PaintShop.Application.DTOs.Sales;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class SaleServiceTests
{
    private static (List<Product> Products, List<ProductVariant> Variants, List<InventoryItem> Items, List<Customer> Customers) SeedData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = new List<ProductVariant>
        {
            new() { Id = 1, ProductId = 1, SizeValue = 1, SizeUnit = "L", SellingPrice = 50, IsActive = true, CreatedAt = DateTime.Now }
        };
        var inventory = new List<InventoryItem>
        {
            new() { Id = 1, ProductVariantId = 1, Location = InventoryLocation.Shop, Quantity = 20 },
            new() { Id = 2, ProductVariantId = 1, Location = InventoryLocation.Warehouse, Quantity = 50 }
        };
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", CreatedAt = DateTime.Now }
        };
        return (products, variants, inventory, customers);
    }

    [Fact]
    public async Task CreateSaleAsync_WithSufficientStock_CreatesSaleAndDeductsStock()
    {
        var (products, variants, inventory, customers) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers);

        // need user for the employee reference
        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var request = new CreateSaleRequest
        {
            CustomerId = 1,
            DiscountAmount = 0,
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductVariantId = 1, Quantity = 3, UnitPrice = 50 }
            }
        };

        var result = await service.CreateSaleAsync(request, 1);

        result.Sale.Should().NotBeNull();
        result.Sale.TotalAmount.Should().Be(150);

        var updatedInventory = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Shop);
        updatedInventory.Quantity.Should().Be(17);
    }

    [Fact]
    public async Task CreateSaleAsync_InsufficientStockWithoutForce_ThrowsException()
    {
        var (products, variants, inventory, customers) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers);

        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var request = new CreateSaleRequest
        {
            CustomerId = 1,
            ForceNegativeInventory = false,
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductVariantId = 1, Quantity = 100, UnitPrice = 50 }
            }
        };

        var act = () => service.CreateSaleAsync(request, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetFilteredAsync_WithDateFilter_ReturnsSalesForDate()
    {
        var (products, variants, inventory, customers) = SeedData();
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 1, CustomerId = 1, TotalAmount = 100, Status = SaleStatus.Active, CreatedAt = today },
            new() { Id = 2, EmployeeId = 1, CustomerId = 1, TotalAmount = 200, Status = SaleStatus.Active, CreatedAt = yesterday }
        };
        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var result = await service.GetFilteredAsync(new SaleFilterRequest { Date = today, Page = 1, PageSize = 20 });

        result.Items.Should().ContainSingle();
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFilteredAsync_WithStatusFilter_ReturnsFilteredSales()
    {
        var (products, variants, inventory, customers) = SeedData();
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 1, CustomerId = 1, TotalAmount = 100, Status = SaleStatus.Active, CreatedAt = DateTime.Now },
            new() { Id = 2, EmployeeId = 1, CustomerId = 1, TotalAmount = 200, Status = SaleStatus.Cancelled, CreatedAt = DateTime.Now }
        };
        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var result = await service.GetFilteredAsync(new SaleFilterRequest { Status = 1, Page = 1, PageSize = 20 });

        result.Items.Should().ContainSingle();
        result.Items[0].Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetFilteredAsync_WithSearch_FiltersByCustomerName()
    {
        var (products, variants, inventory, _) = SeedData();
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Jane Smith", CreatedAt = DateTime.Now }
        };
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 1, CustomerId = 1, TotalAmount = 100, Status = SaleStatus.Active, CreatedAt = DateTime.Now },
            new() { Id = 2, EmployeeId = 1, CustomerId = 2, TotalAmount = 200, Status = SaleStatus.Active, CreatedAt = DateTime.Now }
        };
        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var result = await service.GetFilteredAsync(new SaleFilterRequest { Search = "Jane", Page = 1, PageSize = 20 });

        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_WhenSaleExists_ReturnsSale()
    {
        var (products, variants, inventory, customers) = SeedData();
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 1, CustomerId = 1, TotalAmount = 100, Status = SaleStatus.Active, CreatedAt = DateTime.Now }
        };
        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(100);
    }

    [Fact]
    public async Task CancelSaleAsync_WhenSaleActive_CancelsAndRestoresStock()
    {
        var (products, variants, inventory, customers) = SeedData();
        var saleItems = new List<SaleItem>
        {
            new() { Id = 1, SaleId = 1, ProductVariantId = 1, Quantity = 5, UnitPrice = 50 }
        };
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 1, CustomerId = 1, TotalAmount = 250, Status = SaleStatus.Active, CreatedAt = DateTime.Now, Items = saleItems }
        };
        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        await service.CancelSaleAsync(1, 1);

        var updatedInventory = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Shop);
        updatedInventory.Quantity.Should().Be(25);

        var cancelledSale = await ctx.Sales.FindAsync(1);
        cancelledSale!.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact]
    public async Task CreateSaleAsync_ForceNegative_AllowsNegativeAndWarns()
    {
        var (products, variants, inventory, customers) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers);

        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var request = new CreateSaleRequest
        {
            CustomerId = 1,
            ForceNegativeInventory = true,
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductVariantId = 1, Quantity = 100, UnitPrice = 50 }
            }
        };

        var result = await service.CreateSaleAsync(request, 1);

        result.InventoryWarning.Should().BeTrue();
        result.WarningVariantIds.Should().Contain(1);

        var updatedInventory = await ctx.InventoryItems.FirstAsync(i => i.ProductVariantId == 1 && i.Location == InventoryLocation.Shop);
        updatedInventory.Quantity.Should().Be(-80);
    }

    [Fact]
    public async Task CreateSaleAsync_ResponseContainsPopulatedItems()
    {
        var (products, variants, inventory, customers) = SeedData();
        var ctx = MockDbContext.Create(products: products, variants: variants, inventoryItems: inventory, customers: customers);

        var user = new User { Id = 1, Username = "employee", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" };
        await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();

        var service = new SaleService(ctx);

        var request = new CreateSaleRequest
        {
            CustomerId = 1,
            DiscountAmount = 0,
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductVariantId = 1, Quantity = 3, UnitPrice = 45 }
            }
        };

        var result = await service.CreateSaleAsync(request, 1);

        result.Sale.Items.Should().ContainSingle();
        result.Sale.Items[0].ProductVariantId.Should().Be(1);
        result.Sale.Items[0].Quantity.Should().Be(3);
        result.Sale.Items[0].UnitPrice.Should().Be(45);
        result.Sale.Items[0].OriginalPrice.Should().Be(50);
    }
}
