using Microsoft.EntityFrameworkCore;
using PaintShop.Domain.Entities;

namespace PaintShop.Domain.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<StockTransfer> StockTransfers { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
