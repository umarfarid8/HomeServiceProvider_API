using HomeServiceProvider.Dtos.Customer;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;

    public CustomerService(IUnitOfWork uow) => _uow = uow;

    public async Task<CustomerProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var profile = await _uow.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        return MapToDto(user, profile);
    }

    public async Task<CustomerProfileDto> UpdateProfileAsync(Guid userId, UpdateCustomerProfileDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var profile = await _uow.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Customer profile not found.");

        // Patch-style: only apply fields that were sent
        if (dto.FullName is not null) user.FullName = dto.FullName.Trim();
        if (dto.PhoneNumber is not null) user.PhoneNumber = dto.PhoneNumber.Trim();
        if (dto.City is not null) profile.City = dto.City.Trim();
        if (dto.Address is not null) profile.Address = dto.Address.Trim();
        if (dto.ProfileImageUrl is not null) profile.ProfileImageUrl = dto.ProfileImageUrl;

        _uow.Users.Update(user);
        _uow.CustomerProfiles.Update(profile);
        await _uow.SaveChangesAsync();

        return MapToDto(user, profile);
    }

    // ─── Private Helper ───────────────────────────────────────────────────────

    private static CustomerProfileDto MapToDto(
        DataAccess.Entities.User user,
        DataAccess.Entities.CustomerProfile profile)
        => new()
        {
            ProfileId = profile.Id,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = profile.City,
            Address = profile.Address,
            ProfileImageUrl = profile.ProfileImageUrl,
            IsEmailVerified = user.IsEmailVerified,
            MemberSince = user.CreatedAt
        };
}