using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeServiceProvider.Dtos.Availability;

namespace HomeServiceProvider.Services.Interfaces;

public interface IAvailabilityService
{
    Task SetWeeklyScheduleAsync(Guid userId, SetWeeklyScheduleDto dto);
    Task<List<AvailabilitySlotDto>> GetMyScheduleAsync(Guid userId);
    Task<ProviderAvailabilityResponseDto> GetProviderAvailabilityAsync(Guid providerProfileId, DateTime date);
}