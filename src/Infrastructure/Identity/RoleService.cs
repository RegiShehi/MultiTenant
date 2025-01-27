namespace Infrastructure.Identity;

using System.Security.Claims;
using Application.Exceptions;
using Application.Features.Identity.Roles;
using Constants;
using Finbuckle.MultiTenant;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Persistence.Contexts;
using Tenancy;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    ITenantInfo tenant
)
    : IRoleService
{
    public async Task<string> CreateAsync(CreateRoleRequest request)
    {
        var role = new ApplicationRole()
        {
            Name = request.Name,
            Description = request.Description
        };

        IdentityResult result = await roleManager.CreateAsync(role);

        return !result.Succeeded
            ? throw new IdentityException("Failed to create role", GetIdentityResultErrorDescription(result))
            : role.Name;
    }

    public async Task<string> UpdateAsync(UpdateRoleRequest request)
    {
        ApplicationRole role = await roleManager.FindByIdAsync(request.Id) ??
                               throw new NotFoundException("Role not found");

        if (role.Name != null && RoleConstants.IsDefault(role.Name))
        {
            throw new ConflictException($"Changes not allowed on {role.Name} role");
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.NormalizedName = request.Name.ToUpperInvariant();

        IdentityResult result = await roleManager.UpdateAsync(role);

        return !result.Succeeded
            ? throw new IdentityException("Failed to update role", GetIdentityResultErrorDescription(result))
            : role.Name;
    }

    public async Task<string> DeleteAsync(string id)
    {
        ApplicationRole? role = await roleManager.FindByIdAsync(id);

        if (role?.Name is null)
        {
            throw new NotFoundException("Role not found");
        }

        if (RoleConstants.IsDefault(role.Name))
        {
            throw new ConflictException($"Not allowed to delete {role.Name} role");
        }

        if ((await userManager.GetUsersInRoleAsync(role.Name)).Count > 0)
        {
            throw new ConflictException($"Not allowed to delete {role.Name} role as it is already in use");
        }

        IdentityResult result = await roleManager.DeleteAsync(role);

        return !result.Succeeded
            ? throw new IdentityException(
                $"Failed to delete {role.Name} role",
                GetIdentityResultErrorDescription(result))
            : role.Name;
    }

    public async Task<string> UpdatePermissionsAsync(UpdatePermissionRequest request)
    {
        ApplicationRole role = await roleManager.FindByIdAsync(request.RoleId) ??
                               throw new NotFoundException("Role not found");

        if (role.Name == RoleConstants.Admin)
        {
            throw new ConflictException($"Not allowed to modify permissions for {role.Name} role");
        }

        if (tenant.Id != TenancyConstants.Root.Id)
        {
            request.RemovePermissionsStartingWith("Permission.Root.");
        }

        IList<Claim> claims = await roleManager.GetClaimsAsync(role);

        // remove previously assigned permissions that are not present in incoming request
        foreach (Claim claim in claims.Where(c => request.Permissions.Any(p => p == c.Value)))
        {
            IdentityResult result = await roleManager.RemoveClaimAsync(role, claim);

            if (!result.Succeeded)
            {
                throw new IdentityException("Failed to remove permission", GetIdentityResultErrorDescription(result));
            }
        }

        // assign newly selected permissions
        foreach (string permission in request.Permissions.Where(p => claims.Any(c => c.Value == p)))
        {
            await context.RoleClaims.AddAsync(new IdentityRoleClaim<string>
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = permission
            });

            await context.SaveChangesAsync();
        }

        return "Permissions updated successfully";
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        List<ApplicationRole> roles = await roleManager.Roles.ToListAsync(cancellationToken);

        return roles.Adapt<List<RoleDto>>();
    }

    public async Task<RoleDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken)
    {
        ApplicationRole role = await context.Roles.SingleOrDefaultAsync(r => r.Id == id, cancellationToken) ??
                               throw new NotFoundException("Role not found");

        return role.Adapt<RoleDto>();
    }

    public async Task<RoleDto> GetRoleWithPermissionsAsync(string id, CancellationToken cancellationToken)
    {
        RoleDto roleDto = await GetRoleByIdAsync(id, cancellationToken);

        List<string> permissions = await context.RoleClaims
            .Where(rc => rc.RoleId == id && rc.ClaimType == ClaimConstants.Permission)
            .Select(rc => rc.ClaimValue) // Assuming ClaimValue holds the permission
            .OfType<string>() // Filters out nulls
            .ToListAsync(cancellationToken);

        roleDto.SetPermissions(permissions);

        return roleDto;
    }

    public async Task<bool> DoesItExistAsync(string name, CancellationToken cancellationToken) =>
        await roleManager.RoleExistsAsync(name);

    private static List<string> GetIdentityResultErrorDescription(IdentityResult result) =>
        result.Errors.Select(error => error.Description).ToList();
}
