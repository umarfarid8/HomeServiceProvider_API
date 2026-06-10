using System;
using System.Threading.Tasks;
using HomeServiceProvider.Dtos.Availability;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
        => _availabilityService = availabilityService;

    [HttpPut("schedule")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> SetWeeklySchedule([FromBody] SetWeeklyScheduleDto dto)
    {
        var userId = User.GetUserId();
        await _availabilityService.SetWeeklyScheduleAsync(userId, dto);
        return Ok(new { message = "Weekly schedule updated successfully." });
    }

   
    [HttpGet("my-schedule")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> GetMySchedule()
    {
        var userId = User.GetUserId();
        var schedule = await _availabilityService.GetMyScheduleAsync(userId);
        return Ok(schedule);
    }

    [HttpGet("provider/{providerProfileId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetProviderAvailability(Guid providerProfileId, [FromQuery] DateTime date)
    {
        if (date == default)
            return BadRequest(new { message = "A valid date query parameter is required." });

        var result = await _availabilityService.GetProviderAvailabilityAsync(providerProfileId, date);
        return Ok(result);
    }
}