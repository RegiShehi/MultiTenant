namespace WebApi.Controllers;

using Infrastructure.Identity.Authentication;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Mvc;

internal class Controller1 : Controller
{
    // GET
    [ShouldHavePermission(SchoolAction.View, SchoolFeature.Schools)]
    public IActionResult Index() => View();
}
