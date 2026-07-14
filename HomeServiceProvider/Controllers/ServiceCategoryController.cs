// ServiceCategoryController.cs  (new file)
using HomeServiceProvider.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/service-categories")]
public class ServiceCategoryController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ServiceCategoryController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _uow.ServiceCategories
            .FindAsync(c => c.IsActive);
        return Ok(categories.Select(c => new { c.Id, c.Name, c.Description }));
    }
}