using BCrypt.Net;
using Google.Apis.Auth;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Auth;
using HomeServiceProvider.Helpers;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;
using Microsoft.Extensions.Configuration; // ★ Make sure this using directive is present

namespace HomeServiceProvider.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly JwtTokenGenerator _jwt;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork uow, JwtTokenGenerator jwt, IConfiguration config)
    {
        _uow = uow;
        _jwt = jwt;
        _config = config; // ★ 3. Assign it to your private field
    }
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
    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthDto dto)
    {
        // Step 1: Ask Google to validate the token and return user info
        // If the token is fake or expired, this throws an exception automatically
        GoogleJsonWebSignature.ValidationSettings settings = new()
        {
            Audience = new[] { _config["GoogleAuth:ClientId"] }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch
        {
            throw new UnauthorizedAccessException("Invalid Google token. Please try again.");
        }

        // Step 2: Check if this Google email already has an account
        var existingUser = await _uow.Users.GetByEmailAsync(payload.Email);

        if (existingUser is not null)
        {
            // User already exists — just log them in
            if (!existingUser.IsActive)
                throw new UnauthorizedAccessException("This account has been deactivated.");

            var (token, expiry) = _jwt.GenerateToken(existingUser);
            return BuildAuthResponse(existingUser, token, expiry);
        }

        // Step 3: New user — create account automatically
        // Google already verified their email so IsEmailVerified = true
        var role = dto.Role == "Provider" ? UserRole.Provider : UserRole.Customer;

        var newUser = new User
        {
            FullName = payload.Name ?? payload.Email,
            Email = payload.Email.ToLower(),
            PasswordHash = string.Empty,   // No password — Google handles authentication
            PhoneNumber = string.Empty,
            Role = role,
            IsEmailVerified = true            // Google already verified this
        };

        await _uow.Users.AddAsync(newUser);

        // Create the matching profile based on role
        if (role == UserRole.Customer)
        {
            var customerProfile = new CustomerProfile
            {
                UserId = newUser.Id,
                City = string.Empty,
                Address = string.Empty
            };
            await _uow.CustomerProfiles.AddAsync(customerProfile);
        }
        else
        {
            // Provider — create a minimal profile, they complete it later on their dashboard
            var providerProfile = new ProviderProfile
            {
                UserId = newUser.Id,
                BusinessName = payload.Name ?? "My Business",
                Bio = string.Empty,
                CNIC = string.Empty,
                City = string.Empty,
                VerificationStatus = VerificationStatus.Pending,
                BaseHourlyRate = 0
            };
            await _uow.ProviderProfiles.AddAsync(providerProfile);
        }

        await _uow.SaveChangesAsync();

        var (jwtToken, jwtExpiry) = _jwt.GenerateToken(newUser);
        return BuildAuthResponse(newUser, jwtToken, jwtExpiry);
    }
}