using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.Property(i => i.SubTotal).HasPrecision(18, 2);
            builder.Property(i => i.PlatformCommissionRate).HasPrecision(5, 4);
            builder.Property(i => i.PlatformCommissionAmount).HasPrecision(18, 2);
            builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        }
    }
}
