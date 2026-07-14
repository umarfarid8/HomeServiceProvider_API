using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Two FKs to User — both must be Restrict to avoid multiple cascade paths
            builder.HasOne(r => r.Reviewer)
                   .WithMany(u => u.ReviewsGiven)
                   .HasForeignKey(r => r.ReviewerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Reviewee)
                   .WithMany(u => u.ReviewsReceived)
                   .HasForeignKey(r => r.RevieweeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Booking)
                   .WithMany(b => b.Reviews)
                   .HasForeignKey(r => r.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.AuthenticityScore).HasPrecision(4, 3);
            builder.Property(r => r.Rating).IsRequired();
        }
    }
}
