using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Booking;
using HomeServiceProvider.Dtos.Pricing;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class BookingService : IBookingService
{
    private const int BufferMinutes = 30;
    private readonly IUnitOfWork _uow;
    private readonly IPricingService _pricingService;
    private readonly IInvoiceService _invoiceService;

    public BookingService(IUnitOfWork uow, IPricingService pricingService,
        IInvoiceService invoiceService)
    {
        _uow = uow;
        _pricingService = pricingService;
        _invoiceService = invoiceService;
    }

    public async Task<BookingDto> CreateBookingAsync(Guid customerUserId, CreateBookingDto dto)
    {
        if (dto.ScheduledDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Cannot create a booking for a past date.");
        if (dto.ScheduledStartTime >= dto.ScheduledEndTime)
            throw new InvalidOperationException("Start time must be before end time.");

        var customerProfile = await _uow.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == customerUserId)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        var provider = await _uow.ProviderProfiles.GetByIdAsync(dto.ProviderProfileId)
            ?? throw new KeyNotFoundException("Provider not found.");

        if (!provider.IsVerified)
            throw new InvalidOperationException("This provider has not been verified yet and cannot accept bookings.");
        if (!provider.IsActive)
            throw new InvalidOperationException("This provider account is currently inactive.");

        var category = await _uow.ServiceCategories.GetByIdAsync(dto.ServiceCategoryId)
            ?? throw new KeyNotFoundException("Service category not found.");

        bool offersService = await _uow.ProviderServices.ExistsAsync(ps =>
            ps.ProviderProfileId == dto.ProviderProfileId &&
            ps.ServiceCategoryId == dto.ServiceCategoryId);

        if (!offersService)
            throw new InvalidOperationException($"This provider does not offer {category.Name} services.");

        bool isAvailable = await IsProviderAvailableForSlotAsync(dto.ProviderProfileId, dto.ScheduledDate, dto.ScheduledStartTime, dto.ScheduledEndTime);
        if (!isAvailable)
            throw new InvalidOperationException("The provider is not available on this day or during this time window.");

        var bufferedStart = SafeAddMinutes(dto.ScheduledStartTime, -BufferMinutes);
        var bufferedEnd = SafeAddMinutes(dto.ScheduledEndTime, BufferMinutes);

        bool hasConflict = await _uow.Bookings.HasConflictAsync(dto.ProviderProfileId, dto.ScheduledDate, bufferedStart, bufferedEnd);
        if (hasConflict)
            throw new InvalidOperationException($"This time slot conflicts with an existing booking or its required buffer zone. Please allow at least {BufferMinutes} minutes between bookings.");

        var pricing = await _pricingService.CalculatePriceAsync(dto.ProviderProfileId, dto.ServiceCategoryId, dto.ScheduledDate, dto.ScheduledStartTime, dto.ScheduledEndTime, dto.IsEmergency);

        var booking = new Booking
        {
            CustomerProfileId = customerProfile.Id,
            ProviderProfileId = dto.ProviderProfileId,
            ServiceCategoryId = dto.ServiceCategoryId,
            MatchRequestId = dto.MatchRequestId,
            ProblemDescription = dto.ProblemDescription.Trim(),
            ScheduledDate = dto.ScheduledDate.Date,
            ScheduledStartTime = dto.ScheduledStartTime,
            ScheduledEndTime = dto.ScheduledEndTime,
            Status = BookingStatus.Pending,
            IsEmergency = dto.IsEmergency,
            IsOffHours = pricing.IsOffHours,
            EstimatedAmount = pricing.FinalAmount
        };

        var statusHistory = new BookingStatusHistory
        {
            BookingId = booking.Id,
            Status = BookingStatus.Pending,
            ChangedByUserId = customerUserId,
            Notes = "Booking created."
        };

        var chatThread = new ChatThread { BookingId = booking.Id };

        await _uow.Bookings.AddAsync(booking);
        await _uow.BookingStatusHistories.AddAsync(statusHistory);
        await _uow.ChatThreads.AddAsync(chatThread);
        await _uow.SaveChangesAsync();

        return await GetBookingDtoAsync(booking.Id);
    }
   
    public async Task<List<BookingDto>> GetMyBookingsAsync(Guid userId, string? statusFilter = null)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        List<Booking> bookings = new();

        if (user.Role == UserRole.Provider)
        {
            var providerProfile = await _uow.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (providerProfile != null)
            {
                bookings = (await _uow.Bookings.FindAsync(b => b.ProviderProfileId == providerProfile.Id)).ToList();
            }
        }
        else
        {
            var customerProfile = await _uow.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customerProfile != null)
            {
                bookings = (await _uow.Bookings.FindAsync(b => b.CustomerProfileId == customerProfile.Id)).ToList();
            }
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<BookingStatus>(statusFilter, true, out var parsedStatus))
        {
            bookings = bookings.Where(b => b.Status == parsedStatus).ToList();
        }

        var result = new List<BookingDto>();
        foreach (var b in bookings.OrderByDescending(b => b.CreatedAt))
        {
            try
            {
                var dto = await GetBookingDtoAsync(b.Id);
                result.Add(dto);
            }
            catch
            {
                // Fallback mapping if deep relational include fails
                result.Add(MapToBookingDto(b));
            }
        }

        return result;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, Guid requestingUserId)
    {
        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        bool isCustomer = booking.CustomerProfile?.UserId == requestingUserId;
        bool isProvider = booking.ProviderProfile?.UserId == requestingUserId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException("You don't have permission to view this booking.");

        return MapToBookingDto(booking);
    }

    public async Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid userId, UpdateBookingStatusDto dto)
    {
        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        bool isCustomer = booking.CustomerProfile?.UserId == userId;
        bool isProvider = booking.ProviderProfile?.UserId == userId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException("You don't have permission to update this booking.");

        string actorRole = isCustomer ? "Customer" : "Provider";

        if (!IsValidTransition(booking.Status, dto.NewStatus, actorRole))
            throw new InvalidOperationException($"A {actorRole} cannot transition a booking from '{booking.Status}' to '{dto.NewStatus}'.");

        if (dto.NewStatus == BookingStatus.Cancelled && string.IsNullOrWhiteSpace(dto.CancellationReason))
            throw new InvalidOperationException("A cancellation reason is required.");

        booking.Status = dto.NewStatus;
        if (dto.CancellationReason is not null)
            booking.CancellationReason = dto.CancellationReason.Trim();

        var history = new BookingStatusHistory
        {
            BookingId = booking.Id,
            Status = dto.NewStatus,
            ChangedByUserId = userId,
            Notes = dto.Notes?.Trim()
        };

        _uow.Bookings.Update(booking);
        await _uow.BookingStatusHistories.AddAsync(history);
        await _uow.SaveChangesAsync();

        if (dto.NewStatus == BookingStatus.Completed)
            await _invoiceService.GenerateInvoiceAsync(bookingId);

        return await GetBookingDtoAsync(bookingId);
    }

    private async Task<bool> IsProviderAvailableForSlotAsync(Guid providerProfileId, DateTime date, TimeOnly requestedStart, TimeOnly requestedEnd)
    {
        bool dateBlocked = await _uow.AvailabilitySlots.ExistsAsync(s =>
            s.ProviderProfileId == providerProfileId &&
            !s.IsRecurring &&
            s.SpecificDate.HasValue &&
            s.SpecificDate.Value.Date == date.Date &&
            !s.IsAvailable);

        if (dateBlocked) return false;

        bool hasSlot = await _uow.AvailabilitySlots.ExistsAsync(s =>
            s.ProviderProfileId == providerProfileId &&
            s.IsRecurring &&
            s.DayOfWeek == date.DayOfWeek &&
            s.IsAvailable &&
            s.StartTime <= requestedStart &&
            s.EndTime >= requestedEnd);

        return hasSlot;
    }

    private static bool IsValidTransition(BookingStatus current, BookingStatus next, string actorRole)
        => (current, next, actorRole) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed, "Provider") => true,
            (BookingStatus.Pending, BookingStatus.Cancelled, _) => true,
            (BookingStatus.Confirmed, BookingStatus.InProgress, "Provider") => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled, _) => true,
            (BookingStatus.InProgress, BookingStatus.Completed, "Provider") => true,
            (BookingStatus.InProgress, BookingStatus.Disputed, "Customer") => true,
            (BookingStatus.Completed, BookingStatus.Disputed, "Customer") => true,
            (BookingStatus.Pending, BookingStatus.PendingCancelled, "Provider") => true,
            (BookingStatus.PendingCancelled, BookingStatus.Cancelled, "Provider") => true,
            _ => false
        };

    private async Task<BookingDto> GetBookingDtoAsync(Guid bookingId)
    {
        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found after save.");
        return MapToBookingDto(booking);
    }

    private static BookingDto MapToBookingDto(DataAccess.Entities.Booking booking)
        => new()
        {
            Id = booking.Id,
            CustomerProfileId = booking.CustomerProfileId,
            CustomerName = booking.CustomerProfile?.User?.FullName ?? "Customer",
            ProviderProfileId = booking.ProviderProfileId,
            ProviderName = booking.ProviderProfile?.User?.FullName ?? "Provider",
            BusinessName = booking.ProviderProfile?.BusinessName ?? string.Empty,
            ServiceCategory = booking.ServiceCategory?.Name ?? string.Empty,
            ProblemDescription = booking.ProblemDescription ?? string.Empty,
            ScheduledDate = booking.ScheduledDate.ToString("yyyy-MM-dd"),
            ScheduledStartTime = FormatTime(booking.ScheduledStartTime),
            ScheduledEndTime = FormatTime(booking.ScheduledEndTime),
            Status = booking.Status.ToString(),
            IsEmergency = booking.IsEmergency,
            IsOffHours = booking.IsOffHours,
            EstimatedAmount = booking.EstimatedAmount,
            FinalAmount = booking.FinalAmount,
            CancellationReason = booking.CancellationReason,
            ChatThreadId = booking.ChatThread?.Id ?? Guid.Empty,
            CreatedAt = booking.CreatedAt,
            StatusHistory = booking.StatusHistory?
                .OrderBy(h => h.CreatedAt)
                .Select(h => new StatusHistoryDto
                {
                    Status = h.Status.ToString(),
                    Notes = h.Notes,
                    ChangedAt = h.CreatedAt
                }).ToList() ?? new()
        };

    private static string FormatTime(object time)
    {
        if (time is TimeOnly to) return to.ToString("HH:mm");
        if (time is TimeSpan ts) return ts.ToString(@"hh\:mm");
        return time?.ToString() ?? "00:00";
    }

    private static TimeOnly SafeAddMinutes(TimeOnly time, int minutes)
    {
        var totalMinutes = time.ToTimeSpan().TotalMinutes + minutes;
        totalMinutes = Math.Max(0, Math.Min(totalMinutes, 24 * 60 - 1));
        return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(totalMinutes));
    }
    public async Task DeleteBookingAsync(Guid userId, Guid bookingId)
    {
        var booking = await _uow.Bookings.GetByCustomerIdAsync( bookingId);
        _uow.Bookings.Remove(booking);
        await _uow.SaveChangesAsync();
    }
}