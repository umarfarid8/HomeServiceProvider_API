using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using HomeServiceProvider.DataAccess.Repositories.Specific;
using Microsoft.EntityFrameworkCore.Storage;

namespace HomeServiceProvider.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy-initialized — only created when first accessed
        private IUserRepository? _users;
        private IProviderProfileRepository? _providerProfiles;
        private IBookingRepository? _bookings;
        private IReviewRepository? _reviews;
        private IMatchRequestRepository? _matchRequests;
        private IRepository<CustomerProfile>? _customerProfiles;
        private IRepository<VerificationDocument>? _verificationDocuments;
        private IRepository<ServiceCategory>? _serviceCategories;
        private IRepository<ProviderService>? _providerServices;
        private IRepository<PortfolioImage>? _portfolioImages;
        private IRepository<AvailabilitySlot>? _availabilitySlots;
        private IRepository<BookingStatusHistory>? _bookingStatusHistories;
        private IRepository<ChatThread>? _chatThreads;
        private IRepository<Message>? _messages;
        private IRepository<Invoice>? _invoices;
        private IRepository<PricingRule>? _pricingRules;
        private IRepository<ModerationQueueItem>? _moderationQueueItems;
        private IRepository<MatchResult>? _matchResults;
        private IRepository<AIEvaluationLog>? _aiEvaluationLogs;
        private IRepository<SystemLog>? _systemLogs;

        public UnitOfWork(AppDbContext context) => _context = context;

        // Specific
        public IUserRepository Users
            => _users ??= new UserRepository(_context);
        public IProviderProfileRepository ProviderProfiles
            => _providerProfiles ??= new ProviderProfileRepository(_context);
        public IBookingRepository Bookings
            => _bookings ??= new BookingRepository(_context);
        public IReviewRepository Reviews
            => _reviews ??= new ReviewRepository(_context);
        public IMatchRequestRepository MatchRequests
            => _matchRequests ??= new MatchRequestRepository(_context);

        // Generic
        public IRepository<CustomerProfile> CustomerProfiles
            => _customerProfiles ??= new Repository<CustomerProfile>(_context);
        public IRepository<VerificationDocument> VerificationDocuments
            => _verificationDocuments ??= new Repository<VerificationDocument>(_context);
        public IRepository<ServiceCategory> ServiceCategories
            => _serviceCategories ??= new Repository<ServiceCategory>(_context);
        public IRepository<ProviderService> ProviderServices
            => _providerServices ??= new Repository<ProviderService>(_context);
        public IRepository<PortfolioImage> PortfolioImages
            => _portfolioImages ??= new Repository<PortfolioImage>(_context);
        public IRepository<AvailabilitySlot> AvailabilitySlots
            => _availabilitySlots ??= new Repository<AvailabilitySlot>(_context);
        public IRepository<BookingStatusHistory> BookingStatusHistories
            => _bookingStatusHistories ??= new Repository<BookingStatusHistory>(_context);
        public IRepository<ChatThread> ChatThreads
            => _chatThreads ??= new Repository<ChatThread>(_context);
        public IRepository<Message> Messages
            => _messages ??= new Repository<Message>(_context);
        public IRepository<Invoice> Invoices
            => _invoices ??= new Repository<Invoice>(_context);
        public IRepository<PricingRule> PricingRules
            => _pricingRules ??= new Repository<PricingRule>(_context);
        public IRepository<ModerationQueueItem> ModerationQueueItems
            => _moderationQueueItems ??= new Repository<ModerationQueueItem>(_context);
        public IRepository<MatchResult> MatchResults
            => _matchResults ??= new Repository<MatchResult>(_context);
        public IRepository<AIEvaluationLog> AIEvaluationLogs
            => _aiEvaluationLogs ??= new Repository<AIEvaluationLog>(_context);
        public IRepository<SystemLog> SystemLogs
            => _systemLogs ??= new Repository<SystemLog>(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            await _transaction!.CommitAsync();
            await _transaction.DisposeAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction!.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
