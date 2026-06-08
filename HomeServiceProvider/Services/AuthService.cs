using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Auth;
using HomeServiceProvider.Helpers;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;
using BCrypt.Net;

namespace HomeServiceProvider.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly JwtTokenGenerator _jwt;

    public AuthService(IUnitOfWork uow, JwtTokenGenerator jwt)
    {
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto)
    {
        // Guard: unique email
        if (await _uow.Users.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("This email address is already registered.");

        // Build User + CustomerProfile together — same SaveChanges = same DB transaction
        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Role = UserRole.Customer,
            EmailVerificationToken = Guid.NewGuid().ToString("N")  // 32-char hex token
        };

        var profile = new CustomerProfile
        {
            UserId = user.Id,
            City = dto.City.Trim(),
            Address = dto.Address.Trim()
        };

        await _uow.Users.AddAsync(user);
        await _uow.CustomerProfiles.AddAsync(profile);
        await _uow.SaveChangesAsync();

        // TODO Phase 5: Send verification email using user.EmailVerificationToken

        var (token, expiry) = _jwt.GenerateToken(user);
        return BuildAuthResponse(user, token, expiry);
    }

    public async Task<AuthResponseDto> RegisterProviderAsync(RegisterProviderDto dto)
    {
        if (await _uow.Users.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("This email address is already registered.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Role = UserRole.Provider,
            EmailVerificationToken = Guid.NewGuid().ToString("N")
        };

        var profile = new ProviderProfile
        {
            UserId = user.Id,
            BusinessName = dto.BusinessName.Trim(),
            Bio = dto.Bio.Trim(),
            CNIC = dto.CNIC.Trim(),
            City = dto.City.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ServiceAreaRadiusKm = dto.ServiceAreaRadiusKm,
            BaseHourlyRate = dto.BaseHourlyRate,
            VerificationStatus = VerificationStatus.Pending   // Admin must approve
        };

        await _uow.Users.AddAsync(user);
        await _uow.ProviderProfiles.AddAsync(profile);
        await _uow.SaveChangesAsync();

        var (token, expiry) = _jwt.GenerateToken(user);
        return BuildAuthResponse(user, token, expiry);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email.Trim().ToLower())
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated. Contact support.");

        var (token, expiry) = _jwt.GenerateToken(user);
        return BuildAuthResponse(user, token, expiry);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _uow.Users.GetByVerificationTokenAsync(token);
        if (user is null) return false;

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;  // Consume the token — single use only
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private static AuthResponseDto BuildAuthResponse(User user, string token, DateTime expiry)
        => new()
        {
            Token = token,
            ExpiresAt = expiry,
            UserId = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsEmailVerified = user.IsEmailVerified
        };
}