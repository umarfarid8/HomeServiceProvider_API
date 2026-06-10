using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Availability;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class AvailabilityService : IAvailabilityService
{
    private const int BufferMinutes = 30;
    private readonly IUnitOfWork _uow;

    public AvailabilityService(IUnitOfWork uow) => _uow = uow;

    public async Task SetWeeklyScheduleAsync(Guid userId, SetWeeklyScheduleDto dto)
    {
        ValidateSlots(dto.Slots);
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        await _uow.BeginTransactionAsync();
        try
        {
            var existing = await _uow.AvailabilitySlots.FindAsync(
                s => s.ProviderProfileId == profile.Id && s.IsRecurring);
            _uow.AvailabilitySlots.RemoveRange(existing);
            await _uow.SaveChangesAsync();

            var newSlots = dto.Slots.Select(s => new AvailabilitySlot
            {
                ProviderProfileId = profile.Id,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsRecurring = true,
                IsAvailable = true
            }).ToList();

            await _uow.AvailabilitySlots.AddRangeAsync(newSlots);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();
        }
        catch
        {
            await _uow.RollbackTransactionAsync();
            throw;
        }
    }


    public async Task<List<AvailabilitySlotDto>> GetMyScheduleAsync(Guid userId)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var slots = await _uow.AvailabilitySlots.FindAsync(s => s.ProviderProfileId == profile.Id);

        return slots
            .OrderBy(s => s.IsRecurring ? 0 : 1)
            .ThenBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new AvailabilitySlotDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek.ToString(),
                StartTime = s.StartTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm"),
                IsRecurring = s.IsRecurring,
                SpecificDate = s.SpecificDate,
                IsAvailable = s.IsAvailable
            }).ToList();
    }

    public async Task<ProviderAvailabilityResponseDto> GetProviderAvailabilityAsync(Guid providerProfileId, DateTime date)
    {
        var profile = await _uow.ProviderProfiles.GetWithServicesAndReviewsAsync(providerProfileId)
            ?? throw new KeyNotFoundException("Provider not found.");

        var response = new ProviderAvailabilityResponseDto
        {
            ProviderProfileId = providerProfileId,
            BusinessName = profile.BusinessName,
            Date = date.ToString("yyyy-MM-dd"),
            DayOfWeek = date.DayOfWeek.ToString()
        };

        bool dateBlocked = await _uow.AvailabilitySlots.ExistsAsync(s =>
            s.ProviderProfileId == providerProfileId &&
            !s.IsRecurring &&
            s.SpecificDate.HasValue &&
            s.SpecificDate.Value.Date == date.Date &&
            !s.IsAvailable);

        if (dateBlocked)
        {
            response.IsDayAvailable = false;
            return response;
        }

        var recurringSlot = (await _uow.AvailabilitySlots.FindAsync(s =>
            s.ProviderProfileId == providerProfileId &&
            s.IsRecurring &&
            s.DayOfWeek == date.DayOfWeek &&
            s.IsAvailable)).FirstOrDefault();

        if (recurringSlot is null)
        {
            response.IsDayAvailable = false;
            return response;
        }

        response.IsDayAvailable = true;
        response.WorkingHoursStart = recurringSlot.StartTime.ToString("HH:mm");
        response.WorkingHoursEnd = recurringSlot.EndTime.ToString("HH:mm");

        var existingBookings = await _uow.Bookings.FindAsync(b =>
            b.ProviderProfileId == providerProfileId &&
            b.ScheduledDate.Date == date.Date &&
            b.Status != DataAccess.Enums.BookingStatus.Cancelled);


        return response;
    }

    private static void ValidateSlots(List<WeeklySlotDto> slots)
    {
        foreach (var slot in slots)
        {
            if (slot.StartTime >= slot.EndTime)
                throw new InvalidOperationException($"StartTime must be before EndTime for {slot.DayOfWeek}.");
        }

        var duplicates = slots.GroupBy(s => s.DayOfWeek).Where(g => g.Count() > 1);
        if (duplicates.Any())
            throw new InvalidOperationException("Duplicate day of week entries are not allowed.");
    }

    private static TimeOnly SafeAddMinutes(TimeOnly time, int minutes)
    {
        var totalMinutes = time.ToTimeSpan().TotalMinutes + minutes;
        totalMinutes = Math.Max(0, Math.Min(totalMinutes, 24 * 60 - 1));
        return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(totalMinutes));
    }
}