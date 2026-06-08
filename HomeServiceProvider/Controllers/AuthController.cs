using HomeServiceProvider.Dtos.Auth;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // POST api/auth/register/customer
    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
    {
        var result = await _authService.RegisterCustomerAsync(dto);
        return CreatedAtAction(nameof(RegisterCustomer), result);
    }

    // POST api/auth/register/provider
    [HttpPost("register/provider")]
    public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderDto dto)
    {
        var result = await _authService.RegisterProviderAsync(dto);
        return CreatedAtAction(nameof(RegisterProvider), result);
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    // GET api/auth/verify-email?token=xxxxx
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Verification token is required." });

        var success = await _authService.VerifyEmailAsync(token);
        return success
            ? Ok(new { message = "Email verified successfully. You can now log in." })
            : BadRequest(new { message = "Invalid or expired verification token." });
    }

    // POST api/auth/change-password   (requires login)
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.GetUserId();
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new { message = "Password changed successfully." });
    }
}