using HomeServiceProvider.Dtos.Provider;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/providers")]
//[Authorize(Roles = "Provider")]     // All endpoints require Provider role
public class ProviderController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProviderController(IProviderService providerService)
        => _providerService = providerService;

    // GET api/providers/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        var profile = await _providerService.GetProfileAsync(userId);
        return Ok(profile);
    }

    // PUT api/providers/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProviderProfileDto dto)
    {
        var userId = User.GetUserId();
        var updated = await _providerService.UpdateProfileAsync(userId, dto);
        return Ok(updated);
    }

    // POST api/providers/documents
    [HttpPost("documents")]
    public async Task<IActionResult> AddVerificationDocument([FromBody] AddVerificationDocumentDto dto)
    {
        var userId = User.GetUserId();
        var document = await _providerService.AddVerificationDocumentAsync(userId, dto);
        return CreatedAtAction(nameof(AddVerificationDocument), document);
    }
    // GET api/providers/{providerProfileId}/public
    // Public endpoint — no auth required — used by customer-facing provider profile page
    [HttpGet("{providerProfileId:guid}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(Guid providerProfileId)
    {
        var profile = await _providerService.GetPublicProfileAsync(providerProfileId);
        return Ok(profile);
    }
    // GET api/providers/services
    [HttpGet("services")]
    public async Task<IActionResult> GetMyServices()
    {
        var userId = User.GetUserId();
        var services = await _providerService.GetMyServicesAsync(userId);
        return Ok(services);
    }

    // POST api/providers/services
    [HttpPost("services")]
    public async Task<IActionResult> AddService([FromBody] AddProviderServiceDto dto)
    {
        var userId = User.GetUserId();
        var service = await _providerService.AddServiceAsync(userId, dto);
        return Ok(service);
    }

    // DELETE api/providers/services/{serviceId}
    [HttpDelete("services/{serviceId:guid}")]
    public async Task<IActionResult> RemoveService(Guid serviceId)
    {
        var userId = User.GetUserId();
        await _providerService.RemoveServiceAsync(userId, serviceId);
        return Ok(new { message = "Service removed." });
    }
}