namespace Infrastructure.Identity.Authentication;

using Constants;
using Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ShouldHavePermissionAttribute : AuthorizeAttribute
{
    public ShouldHavePermissionAttribute(string action, string feature)
    {
        Action = action;
        Feature = feature;
        Policy = SchoolPermissionDetails.NameFor(action, feature);
    }

    public string Action { get; }

    public string Feature { get; }
}
