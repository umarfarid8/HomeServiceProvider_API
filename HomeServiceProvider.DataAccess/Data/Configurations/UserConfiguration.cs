using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServiceProvider.DataAccess.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            builder.Property(u => u.PhoneNumber).HasMaxLength(20);

            // One-to-one: User → CustomerProfile
            builder.HasOne(u => u.CustomerProfile)
                   .WithOne(c => c.User)
                   .HasForeignKey<CustomerProfile>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-one: User → ProviderProfile
            builder.HasOne(u => u.ProviderProfile)
                   .WithOne(p => p.User)
                   .HasForeignKey<ProviderProfile>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
