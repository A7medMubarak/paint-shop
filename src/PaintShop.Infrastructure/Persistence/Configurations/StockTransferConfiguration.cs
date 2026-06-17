using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaintShop.Domain.Entities;

namespace PaintShop.Infrastructure.Persistence.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FromLocation).IsRequired();
        builder.Property(t => t.ToLocation).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(300);
        builder.HasOne(t => t.CreatedBy).WithMany().HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}
