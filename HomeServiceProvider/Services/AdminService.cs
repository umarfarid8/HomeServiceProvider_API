using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Admin;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;

    public AdminService(IUnitOfWork uow) => _uow = uow;

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var allUsers = await _uow.Users.GetAllAsync();
        var allBookings = await _uow.Bookings.GetAllAsync();
        var allInvoices = await _uow.Invoices.GetAllAsync();

        var now = DateTime.UtcNow;

        return new AdminDashboardDto
        {
            TotalCustomers = allUsers.Count(u => u.Role == UserRole.Customer),
            TotalProviders = allUsers.Count(u => u.Role == UserRole.Provider),
            TotalAdmins = allUsers.Count(u => u.Role == UserRole.Admin),

            PendingProviderVerifications = (await _uow.ProviderProfiles
                .FindAsync(p => p.VerificationStatus == VerificationStatus.Pending)).Count(),
            PendingModerationItems = (await _uow.ModerationQueueItems
                .FindAsync(m => m.Status == ModerationStatus.Pending)).Count(),
            ActiveDisputes = allBookings.Count(b => b.Status == BookingStatus.Disputed),

            BookingStats = new BookingStatsDto
            {
                Total = allBookings.Count(),
                Pending = allBookings.Count(b => b.Status == BookingStatus.Pending),
                Confirmed = allBookings.Count(b => b.Status == BookingStatus.Confirmed),
                InProgress = allBookings.Count(b => b.Status == BookingStatus.InProgress),
                Completed = allBookings.Count(b => b.Status == BookingStatus.Completed),
                Cancelled = allBookings.Count(b => b.Status == BookingStatus.Cancelled),
                Disputed = allBookings.Count(b => b.Status == BookingStatus.Disputed)
            },

            FinancialSnapshot = new FinancialSnapshotDto
            {
                TotalPlatformRevenue = allInvoices.Sum(i => i.PlatformCommissionAmount),
                ThisMonthRevenue = allInvoices
                    .Where(i => i.CreatedAt.Year == now.Year && i.CreatedAt.Month == now.Month)
                    .Sum(i => i.PlatformCommissionAmount),
                TotalInvoicesGenerated = allInvoices.Count(),
                CashConfirmedInvoices = allInvoices.Count(i => i.IsCashCollected)
            }
        };
    }

    // ── User Management ───────────────────────────────────────────────────────

    public async Task<List<AdminUserDto>> GetUsersAsync(
        string? role, bool? isActive, string? search)
    {
        UserRole? roleEnum = null;
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsed))
            roleEnum = parsed;

        var users = await _uow.Users.SearchUsersAsync(roleEnum, isActive, search);

        return users.Select(u => BuildAdminUserDto(u)).ToList();
    }

    public async Task<AdminUserDto> GetUserDetailAsync(Guid userId)
    {
        var users = await _uow.Users.SearchUsersAsync(null, null, null);
        var user = users.FirstOrDefault(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        return BuildAdminUserDto(user);
    }

    public async Task<AdminUserDto> ToggleUserStatusAsync(Guid targetUserId, Guid adminId)
    {
        var user = await _uow.Users.GetByIdAsync(targetUserId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin accounts cannot be deactivated.");

        user.IsActive = !user.IsActive;
        _uow.Users.Update(user);

        await AddLogAsync(adminId,
            action: user.IsActive ? "USER_ACTIVATED" : "USER_DEACTIVATED",
            entityType: "User",
            entityId: targetUserId.ToString(),
            details: $"Account {(user.IsActive ? "activated" : "deactivated")} for {user.Email}");

        await _uow.SaveChangesAsync();

        return await GetUserDetailAsync(targetUserId);
    }

    // ── Provider Verification ─────────────────────────────────────────────────

    public async Task<List<PendingVerificationDto>> GetPendingVerificationsAsync()
    {
        var providers = await _uow.ProviderProfiles.GetProvidersByCityAsync(string.Empty);

        // GetProvidersByCityAsync filters by city — we need ALL providers, not city-specific
        // Use FindAsync instead with Pending status filter
        var pending = await _uow.ProviderProfiles
            .FindAsync(p => p.VerificationStatus == VerificationStatus.Pending ||
                            p.VerificationStatus == VerificationStatus.UnderReview);

        var result = new List<PendingVerificationDto>();

        foreach (var profile in pending)
        {
            var fullProfile = await _uow.ProviderProfiles.GetFullProfileAsync(profile.Id);
            if (fullProfile is null) continue;

            result.Add(BuildPendingVerificationDto(fullProfile));
        }

        return result.OrderBy(p => p.RegisteredAt).ToList();
    }

    public async Task<PendingVerificationDto> GetVerificationDetailAsync(Guid providerProfileId)
    {
        var profile = await _uow.ProviderProfiles.GetFullProfileAsync(providerProfileId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        return BuildPendingVerificationDto(profile);
    }

    public async Task ApproveVerificationAsync(Guid providerProfileId, Guid adminId)
    {
        var profile = await _uow.ProviderProfiles.GetByIdAsync(providerProfileId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        if (profile.VerificationStatus == VerificationStatus.Approved)
            throw new InvalidOperationException("Provider is already verified.");

        profile.VerificationStatus = VerificationStatus.Approved;
        _uow.ProviderProfiles.Update(profile);

        // Mark all their pending documents as approved too
        var pendingDocs = await _uow.VerificationDocuments
            .FindAsync(d => d.ProviderProfileId == providerProfileId &&
                            d.Status == VerificationStatus.Pending);
        foreach (var doc in pendingDocs)
        {
            doc.Status = VerificationStatus.Approved;
            doc.ReviewedAt = DateTime.UtcNow;
            _uow.VerificationDocuments.Update(doc);
        }

        await AddLogAsync(adminId,
            action: "PROVIDER_VERIFIED",
            entityType: "ProviderProfile",
            entityId: providerProfileId.ToString(),
            details: $"Provider '{profile.BusinessName}' approved for platform");

        await _uow.SaveChangesAsync();
    }

    public async Task RejectVerificationAsync(
        Guid providerProfileId, Guid adminId, VerificationDecisionDto dto)
    {
        var profile = await _uow.ProviderProfiles.GetByIdAsync(providerProfileId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        profile.VerificationStatus = VerificationStatus.Rejected;
        _uow.ProviderProfiles.Update(profile);

        // Mark submitted documents as rejected
        var pendingDocs = await _uow.VerificationDocuments
            .FindAsync(d => d.ProviderProfileId == providerProfileId &&
                            d.Status == VerificationStatus.Pending);
        foreach (var doc in pendingDocs)
        {
            doc.Status = VerificationStatus.Rejected;
            doc.AdminNotes = dto.RejectionReason;
            doc.ReviewedAt = DateTime.UtcNow;
            _uow.VerificationDocuments.Update(doc);
        }

        await AddLogAsync(adminId,
            action: "PROVIDER_REJECTED",
            entityType: "ProviderProfile",
            entityId: providerProfileId.ToString(),
            details: $"Provider '{profile.BusinessName}' rejected. Reason: {dto.RejectionReason}");

        await _uow.SaveChangesAsync();
    }

    // ── Dispute Resolution ────────────────────────────────────────────────────

    public async Task<List<DisputedBookingDto>> GetDisputesAsync()
    {
        var disputes = await _uow.Bookings.GetDisputedBookingsAsync();
        return disputes.Select(BuildDisputedBookingDto).ToList();
    }

    public async Task<DisputedBookingDto> GetDisputeDetailAsync(Guid bookingId)
    {
        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Disputed)
            throw new InvalidOperationException("This booking is not in a disputed state.");

        return BuildDisputedBookingDto(booking);
    }

    public async Task ResolveDisputeAsync(
        Guid bookingId, Guid adminId, ResolveDisputeDto dto)
    {
        if (dto.FinalStatus != BookingStatus.Completed &&
            dto.FinalStatus != BookingStatus.Cancelled)
            throw new InvalidOperationException(
                "Final status must be either Completed or Cancelled.");

        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Disputed)
            throw new InvalidOperationException("Booking is not in a Disputed state.");

        booking.Status = dto.FinalStatus;
        _uow.Bookings.Update(booking);

        // Record the resolution in status history
        var history = new BookingStatusHistory
        {
            BookingId = bookingId,
            Status = dto.FinalStatus,
            ChangedByUserId = adminId,
            Notes = $"[ADMIN RESOLUTION] {dto.Resolution}: {dto.AdminNotes}"
        };
        await _uow.BookingStatusHistories.AddAsync(history);

        // If resolved as Completed and no invoice exists yet, generate one
        if (dto.FinalStatus == BookingStatus.Completed)
        {
            bool invoiceExists = await _uow.Invoices
                .ExistsAsync(i => i.BookingId == bookingId);

            if (!invoiceExists)
            {
                // Build invoice directly here so we don't need to inject InvoiceService
                decimal subTotal = booking.FinalAmount ?? booking.EstimatedAmount;
                decimal commissionAmount = Math.Round(subTotal * 0.15m, 2);
                int count = await _uow.Invoices.CountAsync();

                var invoice = new Invoice
                {
                    BookingId = bookingId,
                    InvoiceNumber = $"HSP-{DateTime.UtcNow.Year}-{(count + 1):D5}",
                    SubTotal = subTotal,
                    PlatformCommissionRate = 0.15m,
                    PlatformCommissionAmount = commissionAmount,
                    TotalAmount = subTotal
                };
                await _uow.Invoices.AddAsync(invoice);
            }
        }

        await AddLogAsync(adminId,
            action: "DISPUTE_RESOLVED",
            entityType: "Booking",
            entityId: bookingId.ToString(),
            details: $"Resolved as {dto.Resolution}. Final status: {dto.FinalStatus}. Notes: {dto.AdminNotes}");

        await _uow.SaveChangesAsync();
    }

    // ── Analytics ─────────────────────────────────────────────────────────────

    public async Task<FinancialAnalyticsDto> GetAnalyticsAsync()
    {
        var allInvoices = (await _uow.Invoices.GetAllAsync()).ToList();
        var allBookings = (await _uow.Bookings.GetAllAsync()).ToList();

        // Monthly breakdown — group by year+month
        var monthly = allInvoices
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyRevenueDto
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Revenue = g.Sum(i => i.PlatformCommissionAmount),
                BookingsCount = g.Count()
            }).ToList();

        // Top providers — by platform commission generated
        var completedBookings = allBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .ToList();

        // Load provider profiles for the top providers query
        var providerStats = completedBookings
            .GroupBy(b => b.ProviderProfileId)
            .Select(g => new
            {
                ProviderProfileId = g.Key,
                CompletedJobs = g.Count()
            })
            .OrderByDescending(x => x.CompletedJobs)
            .Take(10)
            .ToList();

        var topProviders = new List<TopProviderDto>();
        foreach (var stat in providerStats)
        {
            var profile = await _uow.ProviderProfiles.GetByIdAsync(stat.ProviderProfileId);
            var user = profile is not null
                ? await _uow.Users.GetByIdAsync(profile.UserId)
                : null;

            var revenue = allInvoices
                .Where(i => completedBookings
                    .Any(b => b.Id == i.BookingId &&
                              b.ProviderProfileId == stat.ProviderProfileId))
                .Sum(i => i.PlatformCommissionAmount);

            topProviders.Add(new TopProviderDto
            {
                BusinessName = profile?.BusinessName ?? "Unknown",
                ProviderName = user?.FullName ?? "Unknown",
                TotalRevenue = revenue,
                CompletedJobs = stat.CompletedJobs
            });
        }

        // Top categories — by booking count
        var allServiceCategories = await _uow.ServiceCategories.GetAllAsync();
        var topCategories = allBookings
            .GroupBy(b => b.ServiceCategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                TotalBookings = g.Count(),
                Revenue = allInvoices
                    .Where(i => g.Any(b => b.Id == i.BookingId))
                    .Sum(i => i.PlatformCommissionAmount)
            })
            .OrderByDescending(x => x.TotalBookings)
            .Take(5)
            .Select(x => new TopCategoryDto
            {
                CategoryName = allServiceCategories
                    .FirstOrDefault(c => c.Id == x.CategoryId)?.Name ?? "Unknown",
                TotalBookings = x.TotalBookings,
                TotalRevenue = x.Revenue
            }).ToList();

        return new FinancialAnalyticsDto
        {
            TotalTransactionVolume = allInvoices.Sum(i => i.SubTotal),
            TotalPlatformRevenue = allInvoices.Sum(i => i.PlatformCommissionAmount),
            MonthlyBreakdown = monthly,
            TopProviders = topProviders,
            TopCategories = topCategories
        };
    }

    // ── System Logs ───────────────────────────────────────────────────────────

    public async Task<List<SystemLogDto>> GetLogsAsync(
        DateTime? from, DateTime? to, string? action)
    {
        var logs = await _uow.SystemLogs.FindAsync(l =>
            (from == null || l.CreatedAt >= from.Value) &&
            (to == null || l.CreatedAt <= to.Value) &&
            (action == null || l.Action.Contains(action)));

        // Load performer names in memory (avoid complex join)
        var result = new List<SystemLogDto>();
        foreach (var log in logs.OrderByDescending(l => l.CreatedAt))
        {
            string? performerName = null;
            if (log.PerformedByUserId.HasValue)
            {
                var user = await _uow.Users.GetByIdAsync(log.PerformedByUserId.Value);
                performerName = user?.FullName;
            }

            result.Add(new SystemLogDto
            {
                Id = log.Id,
                PerformedBy = performerName ?? "System",
                Action = log.Action,
                TargetEntityType = log.TargetEntityType,
                TargetEntityId = log.TargetEntityId,
                Details = log.Details,
                Timestamp = log.CreatedAt
            });
        }

        return result;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    // Creates an immutable system log entry — called before every SaveChangesAsync
    private async Task AddLogAsync(
        Guid adminId, string action, string entityType,
        string entityId, string? details = null)
    {
        await _uow.SystemLogs.AddAsync(new SystemLog
        {
            PerformedByUserId = adminId,
            Action = action,
            TargetEntityType = entityType,
            TargetEntityId = entityId,
            Details = details
        });
    }

    private static AdminUserDto BuildAdminUserDto(User user)
        => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            JoinedAt = user.CreatedAt,
            BusinessName = user.ProviderProfile?.BusinessName,
            City = user.ProviderProfile?.City ?? user.CustomerProfile?.City,
            VerificationStatus = user.ProviderProfile?.VerificationStatus.ToString(),
            AverageRating = user.ProviderProfile?.AverageRating,
            TotalJobsCompleted = user.ProviderProfile?.TotalJobsCompleted
        };

    private static PendingVerificationDto BuildPendingVerificationDto(ProviderProfile profile)
        => new()
        {
            ProviderProfileId = profile.Id,
            BusinessName = profile.BusinessName,
            ProviderName = profile.User.FullName,
            Email = profile.User.Email,
            PhoneNumber = profile.User.PhoneNumber,
            CNIC = profile.CNIC,
            City = profile.City,
            Bio = profile.Bio,
            BaseHourlyRate = profile.BaseHourlyRate,
            VerificationStatus = profile.VerificationStatus.ToString(),
            RegisteredAt = profile.CreatedAt,
            Documents = profile.VerificationDocuments.Select(d => new AdminDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType.ToString(),
                DocumentUrl = d.DocumentUrl,
                Status = d.Status.ToString(),
                UploadedAt = d.CreatedAt
            }).ToList()
        };

    private static DisputedBookingDto BuildDisputedBookingDto(Booking booking)
    {
        var disputedAt = booking.StatusHistory
            .Where(h => h.Status == BookingStatus.Disputed)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefault()?.CreatedAt ?? booking.CreatedAt;

        return new DisputedBookingDto
        {
            BookingId = booking.Id,
            CustomerName = booking.CustomerProfile.User.FullName,
            CustomerEmail = booking.CustomerProfile.User.Email,
            ProviderName = booking.ProviderProfile.User.FullName,
            BusinessName = booking.ProviderProfile.BusinessName,
            ServiceCategory = booking.ServiceCategory.Name,
            ScheduledDate = booking.ScheduledDate.ToString("yyyy-MM-dd"),
            ScheduledTime = $"{booking.ScheduledStartTime:HH:mm} – {booking.ScheduledEndTime:HH:mm}",
            EstimatedAmount = booking.EstimatedAmount,
            IsEmergency = booking.IsEmergency,
            DisputedAt = disputedAt,

            ChatTranscript = booking.ChatThread?.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new TranscriptMessageDto
                {
                    SenderName = m.Sender.FullName,
                    SenderRole = m.Sender.Role.ToString(),
                    Content = m.Content,
                    SentAt = m.CreatedAt
                }).ToList() ?? new(),

            StatusTimeline = booking.StatusHistory
                .OrderBy(h => h.CreatedAt)
                .Select(h => new StatusChangeDto
                {
                    Status = h.Status.ToString(),
                    Notes = h.Notes,
                    ChangedAt = h.CreatedAt
                }).ToList()
        };
    }
    public async Task<List<SearchAnalyticsLogDto>> GetFailedSearchesAsync()
    {
        // Return only the failed (low-confidence) searches sorted by recency
        // Admin uses this to identify missing service categories
        var logs = await _uow.SearchAnalyticsLogs
            .FindAsync(l => !l.WasSuccessful);

        return logs
            .OrderByDescending(l => l.CreatedAt)
            .Take(200)    // last 200 failed searches
            .Select(l => new SearchAnalyticsLogDto
            {
                Id = l.Id,
                RawQuery = l.RawQuery,
                ClassifiedCategory = l.ClassifiedCategory,
                ConfidenceScore = l.ConfidenceScore,
                WasSuccessful = l.WasSuccessful,
                FailureReason = l.FailureReason,
                ServedFromCache = l.ServedFromCache,
                ProvidersReturned = l.ProvidersReturned,
                SearchedAt = l.CreatedAt
            }).ToList();
    }
    public async Task<bool> CreateServiceCategoryAsync(CreateServiceCategoryDto dto)
    {
        // 1. Check if the category already exists (case-insensitive)
        var normalizedName = dto.Name.Trim();
        var exists = await _uow.ServiceCategories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedName.ToLower());

        if (exists != null)
        {
            throw new InvalidOperationException($"The category '{normalizedName}' already exists.");
        }

        // 2. Map and instantiate the entity (including audit columns)
        var newCategory = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = dto.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Commit to SQL Server database
        await _uow.ServiceCategories.AddAsync(newCategory);
        return await _uow.SaveChangesAsync() > 0;
    }
}
