namespace WebApi.Controllers;

using Infrastructure.Identity.Authentication;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    // GET
    [HttpGet]
    [ShouldHavePermission(SchoolAction.View, SchoolFeature.Schools)]
    public async Task<IActionResult> Get()
    {
        await Task.Delay(1000); // 1000 milliseconds = 1 second
        return Ok("Hello World!");
    }
}
