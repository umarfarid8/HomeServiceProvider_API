using HomeServiceProvider.Dtos.Auth;

namespace HomeServiceProvider.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task<AuthResponseDto> RegisterProviderAsync(RegisterProviderDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    // Add this line to IAuthService
    Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthDto dto);
    Task<bool> VerifyEmailAsync(string token);
}