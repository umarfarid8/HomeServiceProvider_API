using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // Multiple FKs on Booking — restrict to prevent multiple cascade paths
            builder.HasOne(b => b.CustomerProfile)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CustomerProfileId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ProviderProfile)
                   .WithMany(p => p.Bookings)
                   .HasForeignKey(b => b.ProviderProfileId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ServiceCategory)
                   .WithMany(s => s.Bookings)
                   .HasForeignKey(b => b.ServiceCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.MatchRequest)
                   .WithMany(m => m.Bookings)
                   .HasForeignKey(b => b.MatchRequestId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(b => b.EstimatedAmount).HasPrecision(18, 2);
            builder.Property(b => b.FinalAmount).HasPrecision(18, 2);
        }
    }
}
