using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaintShop.Domain.Entities;

namespace PaintShop.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.BaseType).IsRequired(false);
        builder.Property(v => v.SizeValue).HasPrecision(5, 2).IsRequired();
        builder.Property(v => v.SizeUnit).HasMaxLength(10).HasDefaultValue("L").IsRequired();
        builder.Property(v => v.SellingPrice).HasPrecision(10, 2);
        builder.Property(v => v.CostPrice).HasPrecision(10, 2);
        builder.Property(v => v.LowStockThreshold).HasPrecision(5, 2);
        builder.Property(v => v.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(v => v.CreatedAt).IsRequired();
        builder.HasIndex(v => new { v.ProductId, v.BaseType, v.SizeValue, v.SizeUnit }).IsUnique();
        builder.HasOne(v => v.Product).WithMany(p => p.Variants).HasForeignKey(v => v.ProductId);
    }
}
