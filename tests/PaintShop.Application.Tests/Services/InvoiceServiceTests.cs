using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Tests.TestCommon;

namespace PaintShop.Application.Tests.Services;

public class InvoiceServiceTests
{
    private static IApplicationDbContext Seed(string customerName = "John Doe")
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "emp", Role = UserRole.Employee, IsActive = true, CreatedAt = DateTime.Now, PasswordHash = "" }
        };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Paint A", ProductCategory = ProductCategory.GlcPlastic, IsActive = true, CreatedAt = DateTime.Now }
        };
        var variants = new List<ProductVariant>
        {
            new() { Id = 1, ProductId = 1, BaseType = BaseType.BaseA, SizeValue = 1, SizeUnit = "L", SellingPrice = 50, IsActive = true, CreatedAt = DateTime.Now }
        };
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = customerName, Phone = "0100", CreatedAt = DateTime.Now }
        };
        var sales = new List<Sale>
        {
            new() { Id = 1, EmployeeId = 1, CustomerId = 1, TotalAmount = 100, DiscountAmount = 0, Status = SaleStatus.Active, CreatedAt = DateTime.Now }
        };
        var saleItems = new List<SaleItem>
        {
            new() { Id = 1, SaleId = 1, ProductVariantId = 1, Quantity = 2, UnitPrice = 50 }
        };
        return MockDbContext.Create(users: users, products: products, variants: variants, customers: customers, sales: sales, saleItems: saleItems);
    }

    [Fact]
    public async Task GenerateHtmlAsync_WithValidSale_ContainsTotals()
    {
        var ctx = Seed();
        var service = new InvoiceService(ctx);

        var html = await service.GenerateHtmlAsync(1);

        html.Should().Contain("Invoice #1");
        html.Should().Contain("John Doe");
        html.Should().Contain("100.00");
    }

    [Fact]
    public async Task GenerateHtmlAsync_WithMissingSale_ThrowsNotFound()
    {
        var ctx = Seed();
        var service = new InvoiceService(ctx);

        var act = () => service.GenerateHtmlAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GenerateHtmlAsync_EncodesMaliciousNames()
    {
        var ctx = Seed("<script>alert(1)</script>");
        var service = new InvoiceService(ctx);

        var html = await service.GenerateHtmlAsync(1);

        html.Should().NotContain("<script>alert(1)</script>");
        html.Should().Contain("&lt;script&gt;");
    }
}
