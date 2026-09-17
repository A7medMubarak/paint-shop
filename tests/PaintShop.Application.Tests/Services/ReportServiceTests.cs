using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class ReportServiceTests
{
    private static (List<User> Users, List<Product> Products, List<ProductVariant> Variants, List<InventoryItem> Inventory, List<Customer> Customers) SeedCatalog()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "owner", Role = UserRole.Owner, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" },
            new() { Id = 2, Username = "emp", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = new List<ProductVariant>
        {
            new() { Id = 1, ProductId = 1, BaseType = BaseType.BaseA, SizeValue = 1, SizeUnit = "L", SellingPrice = 50, CostPrice = 30, LowStockThreshold = 5, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, ProductId = 1, BaseType = BaseType.BaseB, SizeValue = 4, SizeUnit = "L", SellingPrice = 100, CostPrice = null, LowStockThreshold = 5, IsActive = true, CreatedAt = DateTime.Now }
        };
        var inventory = new List<InventoryItem>
        {
            new() { Id = 1, ProductVariantId = 1, Location = InventoryLocation.Shop, Quantity = 3 },
            new() { Id = 2, ProductVariantId = 1, Location = InventoryLocation.Warehouse, Quantity = 10 },
            new() { Id = 3, ProductVariantId = 2, Location = InventoryLocation.Shop, Quantity = 20 },
            new() { Id = 4, ProductVariantId = 2, Location = InventoryLocation.Warehouse, Quantity = 0 }
        };
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John", CreatedAt = DateTime.Now }
        };
        return (users, products, variants, inventory, customers);
    }

    [Fact]
    public async Task GetDailyReportAsync_ExcludesCancelledFromRevenue()
    {
        var (users, products, variants, inventory, customers) = SeedCatalog();
        var today = DateTime.Today.AddHours(10);
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 2, CustomerId = 1, TotalAmount = 100, DiscountAmount = 10, Status = SaleStatus.Active, CreatedAt = today },
            new() { Id = 2, EmployeeId = 2, CustomerId = 1, TotalAmount = 200, DiscountAmount = 0, Status = SaleStatus.Active, CreatedAt = today },
            new() { Id = 3, EmployeeId = 2, CustomerId = 1, TotalAmount = 500, DiscountAmount = 0, Status = SaleStatus.Cancelled, CreatedAt = today },
            new() { Id = 4, EmployeeId = 2, CustomerId = 1, TotalAmount = 999, DiscountAmount = 0, Status = SaleStatus.Active, CreatedAt = today.AddDays(-1) }
        };
        var ctx = MockDbContext.Create(users: users, products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        var service = new ReportService(ctx);

        var result = await service.GetDailyReportAsync(today);

        result.TotalSalesCount.Should().Be(2);
        result.TotalRevenue.Should().Be(300);
        result.TotalDiscountsGiven.Should().Be(10);
        result.CancelledSalesCount.Should().Be(1);
        result.SalesByEmployee.Should().ContainSingle().Which.Revenue.Should().Be(300);
    }

    [Fact]
    public async Task GetTopSellingAsync_ExcludesCancelledSales()
    {
        var (users, products, variants, inventory, customers) = SeedCatalog();
        var today = DateTime.Today.AddHours(10);
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 2, CustomerId = 1, TotalAmount = 100, Status = SaleStatus.Active, CreatedAt = today },
            new() { Id = 2, EmployeeId = 2, CustomerId = 1, TotalAmount = 500, Status = SaleStatus.Cancelled, CreatedAt = today }
        };
        var saleItems = new List<SaleItem>
        {
            new() { Id = 1, SaleId = 1, ProductVariantId = 1, Quantity = 2, UnitPrice = 50 },
            new() { Id = 2, SaleId = 2, ProductVariantId = 1, Quantity = 100, UnitPrice = 50 }
        };
        var ctx = MockDbContext.Create(users: users, products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales, saleItems: saleItems);
        var service = new ReportService(ctx);

        var result = await service.GetTopSellingAsync(today.AddDays(-7), today);

        result.Should().ContainSingle();
        result[0].QuantitySold.Should().Be(2);
        result[0].Revenue.Should().Be(100);
    }

    [Fact]
    public async Task GetLowStockReportAsync_FlagsOnlyBelowThreshold()
    {
        var (users, products, variants, inventory, customers) = SeedCatalog();
        var ctx = MockDbContext.Create(users: users, products: products, variants: variants, inventoryItems: inventory, customers: customers);
        var service = new ReportService(ctx);

        var result = await service.GetLowStockReportAsync();

        result.Should().ContainSingle();
        result[0].VariantId.Should().Be(1);
        result[0].ShopStock.Should().Be(3);
        result[0].Threshold.Should().Be(5);
    }

    [Fact]
    public async Task GetInventoryValuationAsync_UsesCostPriceFirst()
    {
        var (users, products, variants, inventory, customers) = SeedCatalog();
        var ctx = MockDbContext.Create(users: users, products: products, variants: variants, inventoryItems: inventory, customers: customers);
        var service = new ReportService(ctx);

        var result = await service.GetInventoryValuationAsync();

        var shop = result.Should().ContainSingle(r => r.Location == "Shop").Subject;
        shop.TotalValue.Should().Be(3 * 30 + 20 * 100);
        var warehouse = result.Should().ContainSingle(r => r.Location == "Warehouse").Subject;
        warehouse.TotalValue.Should().Be(10 * 30 + 0);
    }

    [Fact]
    public async Task GetPeriodReportAsync_FiltersByDateRange()
    {
        var (users, products, variants, inventory, customers) = SeedCatalog();
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 2, CustomerId = 1, TotalAmount = 100, Status = SaleStatus.Active, CreatedAt = DateTime.Today.AddDays(-5) },
            new() { Id = 2, EmployeeId = 2, CustomerId = 1, TotalAmount = 200, Status = SaleStatus.Active, CreatedAt = DateTime.Today.AddDays(-40) }
        };
        var ctx = MockDbContext.Create(users: users, products: products, variants: variants, inventoryItems: inventory, customers: customers, sales: sales);
        var service = new ReportService(ctx);

        var result = await service.GetPeriodReportAsync(DateTime.Today.AddDays(-30), DateTime.Today);

        result.TotalSalesCount.Should().Be(1);
        result.TotalRevenue.Should().Be(100);
    }
}
