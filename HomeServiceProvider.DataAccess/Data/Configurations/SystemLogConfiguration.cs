using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations
{
    public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
    {
        public void Configure(EntityTypeBuilder<SystemLog> builder)
        {
            // Nullable FK — log survives if the user is deleted
            builder.HasOne(l => l.PerformedByUser)
                   .WithMany(u => u.SystemLogs)
                   .HasForeignKey(l => l.PerformedByUserId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(l => l.Action).HasMaxLength(200).IsRequired();
            builder.Property(l => l.TargetEntityType).HasMaxLength(100);
        }
    }
}
