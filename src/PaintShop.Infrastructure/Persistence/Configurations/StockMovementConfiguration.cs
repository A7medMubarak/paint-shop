using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaintShop.Domain.Entities;

namespace PaintShop.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Location).IsRequired();
        builder.Property(m => m.QuantityChange).HasPrecision(10, 2).IsRequired();
        builder.Property(m => m.Reason).IsRequired();
        builder.Property(m => m.Notes).HasMaxLength(500).IsRequired(false);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.HasOne(m => m.Variant).WithMany().HasForeignKey(m => m.ProductVariantId);
        builder.HasOne(m => m.Sale).WithMany().HasForeignKey(m => m.ReferenceSaleId).IsRequired(false);
        builder.HasOne(m => m.StockTransfer).WithMany(t => t.Movements).HasForeignKey(m => m.StockTransferId).IsRequired(false);
        builder.HasOne(m => m.CreatedBy).WithMany().HasForeignKey(m => m.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}
