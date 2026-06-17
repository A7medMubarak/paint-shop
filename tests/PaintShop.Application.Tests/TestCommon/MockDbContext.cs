namespace PaintShop.Application.Tests.TestCommon;

public static class MockDbContext
{
    public static IApplicationDbContext Create(
        List<User>? users = null,
        List<Product>? products = null,
        List<ProductVariant>? variants = null,
        List<InventoryItem>? inventoryItems = null,
        List<Customer>? customers = null,
        List<Sale>? sales = null,
        List<SaleItem>? saleItems = null,
        List<StockMovement>? stockMovements = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        if (users != null) context.Users.AddRange(users);
        if (products != null) context.Products.AddRange(products);
        if (variants != null) context.ProductVariants.AddRange(variants);
        if (inventoryItems != null) context.InventoryItems.AddRange(inventoryItems);
        if (customers != null) context.Customers.AddRange(customers);
        if (sales != null) context.Sales.AddRange(sales);
        if (saleItems != null) context.SaleItems.AddRange(saleItems);
        if (stockMovements != null) context.StockMovements.AddRange(stockMovements);

        context.SaveChanges();
        return context;
    }
}
