using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HomeServiceProvider.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Identity
        public DbSet<User> Users => Set<User>();
        public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
        public DbSet<ProviderProfile> ProviderProfiles => Set<ProviderProfile>();
        public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();

        // Catalog
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<ProviderService> ProviderServices => Set<ProviderService>();
        public DbSet<PortfolioImage> PortfolioImages => Set<PortfolioImage>();

        // Scheduling
        public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();

        // Messaging
        public DbSet<ChatThread> ChatThreads => Set<ChatThread>();
        public DbSet<Message> Messages => Set<Message>();

        // Transactions
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<PricingRule> PricingRules => Set<PricingRule>();

        // Reviews
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<ModerationQueueItem> ModerationQueueItems => Set<ModerationQueueItem>();

        // AI Matching
        public DbSet<MatchRequest> MatchRequests => Set<MatchRequest>();
        public DbSet<MatchResult> MatchResults => Set<MatchResult>();
        public DbSet<AIEvaluationLog> AIEvaluationLogs => Set<AIEvaluationLog>();

        // Admin
        public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Auto-discovers all IEntityTypeConfiguration<T> classes in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-stamp UpdatedAt on every modified BaseEntity
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
