using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaintShop.Domain.Entities;

namespace PaintShop.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.DiscountAmount).HasPrecision(10, 2).HasDefaultValue(0).IsRequired();
        builder.Property(s => s.TotalAmount).HasPrecision(10, 2).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.HasOne(s => s.Employee).WithMany().HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(s => s.Customer).WithMany(c => c.Sales).HasForeignKey(s => s.CustomerId);
    }
}
