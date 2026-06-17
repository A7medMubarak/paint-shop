using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaintShop.Domain.Entities;

namespace PaintShop.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Location).IsRequired();
        builder.Property(i => i.Quantity).HasPrecision(10, 2).HasDefaultValue(0).IsRequired();
        builder.HasIndex(i => new { i.ProductVariantId, i.Location }).IsUnique();
        builder.HasOne(i => i.Variant).WithMany().HasForeignKey(i => i.ProductVariantId);
    }
}
