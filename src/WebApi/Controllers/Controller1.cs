namespace WebApi.Controllers;

using Infrastructure.Identity.Authentication;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Mvc;

internal sealed class Controller1 : Controller
{
    // GET
    [ShouldHavePermission(SchoolAction.View, SchoolFeature.Schools)]
    public IActionResult Index() => Ok("Hello World!");
}
