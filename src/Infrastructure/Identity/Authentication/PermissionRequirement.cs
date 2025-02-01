namespace Infrastructure.Identity.Authentication;

using Microsoft.AspNetCore.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; set; } = permission;
}
