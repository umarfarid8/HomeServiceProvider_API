using HomeServiceProvider.Dtos.Customer;

namespace HomeServiceProvider.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto> GetProfileAsync(Guid userId);
    Task<CustomerProfileDto> UpdateProfileAsync(Guid userId, UpdateCustomerProfileDto dto);
}