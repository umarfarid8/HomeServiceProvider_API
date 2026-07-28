using System;
using System.Threading.Tasks;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Booking;
using HomeServiceProvider.Dtos.Pricing;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
        => _bookingService = bookingService;

    // ── Create Booking ───────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var userId = User.GetUserId();
        var booking = await _bookingService.CreateBookingAsync(userId, dto);
        return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
    }

    // ── Get All Bookings for Logged-In User (FIXED: Missing Endpoint) ──────
    [HttpGet]
    public async Task<IActionResult> GetMyBookings([FromQuery] string? status)
    {
        var userId = User.GetUserId();
        var bookings = await _bookingService.GetMyBookingsAsync(userId, status);
        return Ok(bookings);
    }

    // ── Get Single Booking Details ───────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Customer,Provider")]
    public async Task<IActionResult> GetBookingById(Guid id)
    {
        var userId = User.GetUserId();
        var booking = await _bookingService.GetBookingByIdAsync(id, userId);
        return Ok(booking);
    }

    // ── Update Booking Status (Confirm / Cancel / Complete) ─────────────────
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Customer,Provider")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusDto dto)
    {
        var userId = User.GetUserId();
        var booking = await _bookingService.UpdateStatusAsync(id, userId, dto);
        return Ok(booking);
    }


    [HttpDelete ("{Id:guid}")]
   [Authorize(Roles = "Customer,Provider")]
    public async Task<IActionResult> DeleteBooking(Guid bookingId)
    {
        var userId = User.GetUserId();
        await _bookingService.DeleteBookingAsync(userId , bookingId);
        return Ok(new {Message = "Booking Deleted"});
    }
}