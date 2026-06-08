using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations
{
    public class ProviderProfileConfiguration : IEntityTypeConfiguration<ProviderProfile>
    {
        public void Configure(EntityTypeBuilder<ProviderProfile> builder)
        {
            // Computed property — do not map to a column
            builder.Ignore(p => p.IsVerified);

            builder.Property(p => p.AverageRating).HasPrecision(3, 2);
            builder.Property(p => p.BaseHourlyRate).HasPrecision(18, 2);
            builder.Property(p => p.Bio).HasMaxLength(1000);
        }
    }
}
