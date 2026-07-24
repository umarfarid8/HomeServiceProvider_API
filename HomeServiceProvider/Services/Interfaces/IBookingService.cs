using HomeServiceProvider.Dtos.Booking;

namespace HomeServiceProvider.Services.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid customerUserId, CreateBookingDto dto);

    // 👇 Added missing method signature
    Task<List<BookingDto>> GetMyBookingsAsync(Guid userId, string? statusFilter = null);

    Task<BookingDto> GetBookingByIdAsync(Guid bookingId, Guid requestingUserId);
    Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid userId, UpdateBookingStatusDto dto);
}