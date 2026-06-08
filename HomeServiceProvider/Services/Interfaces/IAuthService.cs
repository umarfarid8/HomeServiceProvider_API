using HomeServiceProvider.Dtos.Auth;

namespace HomeServiceProvider.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task<AuthResponseDto> RegisterProviderAsync(RegisterProviderDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<bool> VerifyEmailAsync(string token);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}