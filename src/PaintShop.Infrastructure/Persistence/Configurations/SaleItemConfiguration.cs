using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaintShop.Domain.Entities;

namespace PaintShop.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity).HasPrecision(10, 2).IsRequired();
        builder.Property(i => i.OriginalPrice).HasPrecision(10, 2);
        builder.Property(i => i.UnitPrice).HasPrecision(10, 2).IsRequired();
        builder.Property(i => i.ColorCode).HasMaxLength(50);
        builder.HasOne(i => i.Sale).WithMany(s => s.Items).HasForeignKey(i => i.SaleId);
        builder.HasOne(i => i.Variant).WithMany().HasForeignKey(i => i.ProductVariantId);
    }
}
