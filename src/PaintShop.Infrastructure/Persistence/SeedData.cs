using Microsoft.EntityFrameworkCore;
using PaintShop.Domain.Entities;
using PaintShop.Domain.Enums;

namespace PaintShop.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        context.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Owner,
            CreatedAt = DateTime.Now
        });

        context.Customers.Add(new Customer
        {
            Name = "Walk-in",
            CreatedAt = DateTime.Now
        });

        var plastic = new Product
        {
            Name = "GLC Plastic",
            ProductCategory = ProductCategory.GlcPlastic,
            CreatedAt = DateTime.Now
        };
        var decore = new Product
        {
            Name = "GLC Decore",
            ProductCategory = ProductCategory.GlcDecore,
            CreatedAt = DateTime.Now
        };
        var oilBased = new Product
        {
            Name = "GLC Oil Based",
            ProductCategory = ProductCategory.GlcOilBased,
            CreatedAt = DateTime.Now
        };
        context.Products.AddRange(plastic, decore, oilBased);
        await context.SaveChangesAsync();

        var plasticVariants = new List<ProductVariant>();
        foreach (var (baseType, size) in new[] {
            (BaseType.BaseA, 1m), (BaseType.BaseA, 4m), (BaseType.BaseA, 9m),
            (BaseType.BaseB, 1m), (BaseType.BaseB, 4m), (BaseType.BaseB, 9m),
            (BaseType.BaseC, 1m), (BaseType.BaseC, 4m), (BaseType.BaseC, 9m)
        })
        {
            plasticVariants.Add(new ProductVariant
            {
                ProductId = plastic.Id,
                BaseType = baseType,
                SizeValue = size,
                SizeUnit = "L",
                SellingPrice = size switch
                {
                    1m => 80m,
                    4m => 250m,
                    9m => 500m,
                    _ => null
                },
                LowStockThreshold = 5,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        var decoreVariants = new List<ProductVariant>();
        foreach (var (baseType, size) in new[] {
            (BaseType.BaseA, 1m), (BaseType.BaseA, 4m), (BaseType.BaseA, 9m),
            (BaseType.BaseB, 1m), (BaseType.BaseB, 4m), (BaseType.BaseB, 9m),
            (BaseType.BaseC, 1m), (BaseType.BaseC, 4m), (BaseType.BaseC, 9m)
        })
        {
            decoreVariants.Add(new ProductVariant
            {
                ProductId = decore.Id,
                BaseType = baseType,
                SizeValue = size,
                SizeUnit = "L",
                SellingPrice = size switch
                {
                    1m => 90m,
                    4m => 300m,
                    9m => 600m,
                    _ => null
                },
                LowStockThreshold = 5,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        var oilVariants = new List<ProductVariant>();
        foreach (var (baseType, size) in new[] {
            (BaseType.BaseA, 1m), (BaseType.BaseA, 4m),
            (BaseType.BaseB, 1m), (BaseType.BaseB, 4m)
        })
        {
            oilVariants.Add(new ProductVariant
            {
                ProductId = oilBased.Id,
                BaseType = baseType,
                SizeValue = size,
                SizeUnit = "L",
                SellingPrice = size switch
                {
                    1m => 120m,
                    4m => 400m,
                    _ => null
                },
                LowStockThreshold = 5,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        context.ProductVariants.AddRange(plasticVariants);
        context.ProductVariants.AddRange(decoreVariants);
        context.ProductVariants.AddRange(oilVariants);
        await context.SaveChangesAsync();

        var allVariants = plasticVariants.Concat(decoreVariants).Concat(oilVariants).ToList();
        var inventoryItems = new List<InventoryItem>();
        foreach (var variant in allVariants)
        {
            inventoryItems.Add(new InventoryItem
            {
                ProductVariantId = variant.Id,
                Location = InventoryLocation.Shop,
                Quantity = 0
            });
            inventoryItems.Add(new InventoryItem
            {
                ProductVariantId = variant.Id,
                Location = InventoryLocation.Warehouse,
                Quantity = 0
            });
        }
        context.InventoryItems.AddRange(inventoryItems);
        await context.SaveChangesAsync();
    }
}
