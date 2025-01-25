using System.Security.Claims;
using Infrastructure.Identity.Constants;

namespace Infrastructure.Identity;

public static class ClaimPrincipalExtensions
{
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }

    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    public static string GetFirstName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }

    public static string GetLastName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
    }

    public static string GetPhoneNumber(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty;
    }

    public static string GetTenant(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimConstants.Tenant) ?? string.Empty;
    }
}