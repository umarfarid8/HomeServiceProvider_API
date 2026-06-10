using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeServiceProvider.Dtos.Booking;
using HomeServiceProvider.Dtos.Pricing;

namespace HomeServiceProvider.Services.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid customerUserId, CreateBookingDto dto);
    Task<BookingDto> GetBookingByIdAsync(Guid bookingId, Guid requestingUserId);
    Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid userId, UpdateBookingStatusDto dto);
}