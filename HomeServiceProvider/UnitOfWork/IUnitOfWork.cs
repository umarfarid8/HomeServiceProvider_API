using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using HomeServiceProvider.DataAccess.Repositories.Specific;

namespace HomeServiceProvider.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // Specific (domain-rich) repositories
        IUserRepository Users { get; }
        IProviderProfileRepository ProviderProfiles { get; }
        IBookingRepository Bookings { get; }
        IReviewRepository Reviews { get; }
        IMatchRequestRepository MatchRequests { get; }

        // Generic repositories (no custom queries needed)
        IRepository<CustomerProfile> CustomerProfiles { get; }
        IRepository<VerificationDocument> VerificationDocuments { get; }
        IRepository<ServiceCategory> ServiceCategories { get; }
        IRepository<ProviderService> ProviderServices { get; }
        IRepository<PortfolioImage> PortfolioImages { get; }
        IRepository<AvailabilitySlot> AvailabilitySlots { get; }
        IRepository<BookingStatusHistory> BookingStatusHistories { get; }
        IChatThreadRepository ChatThreads { get; }
        IRepository<Message> Messages { get; }
        IRepository<Invoice> Invoices { get; }
        IRepository<PricingRule> PricingRules { get; }
        IRepository<ModerationQueueItem> ModerationQueueItems { get; }
        IRepository<MatchResult> MatchResults { get; }
        IRepository<AIEvaluationLog> AIEvaluationLogs { get; }
        IRepository<SystemLog> SystemLogs { get; }

        // Persistence
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Explicit transaction control (use for multi-step operations)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
