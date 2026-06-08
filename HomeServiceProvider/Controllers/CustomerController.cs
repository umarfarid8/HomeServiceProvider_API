using HomeServiceProvider.Dtos.Customer;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Customer")]     // All endpoints in this controller require Customer role
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
        => _customerService = customerService;

    // GET api/customers/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        var profile = await _customerService.GetProfileAsync(userId);
        return Ok(profile);
    }

    // PUT api/customers/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileDto dto)
    {
        var userId = User.GetUserId();
        var updated = await _customerService.UpdateProfileAsync(userId, dto);
        return Ok(updated);
    }
}