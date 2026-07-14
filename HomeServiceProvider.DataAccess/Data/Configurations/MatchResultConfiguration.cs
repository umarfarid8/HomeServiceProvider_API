using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations;

public class MatchResultConfiguration : IEntityTypeConfiguration<MatchResult>
{
    public void Configure(EntityTypeBuilder<MatchResult> builder)
    {
        builder.HasOne(r => r.MatchRequest)
               .WithMany(m => m.MatchResults)
               .HasForeignKey(r => r.MatchRequestId)
               .OnDelete(DeleteBehavior.Cascade);

        // CRITICAL: Restrict prevents the multiple cascade paths loop crash
        builder.HasOne(r => r.ProviderProfile)
               .WithMany(p => p.MatchResults)
               .HasForeignKey(r => r.ProviderProfileId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.AIScore).HasPrecision(5, 2);
    }
}