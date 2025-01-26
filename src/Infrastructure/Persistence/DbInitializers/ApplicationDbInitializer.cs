using System.Security.Claims;
using Infrastructure.Identity.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.DbInitializers;

public class ApplicationDbInitializer(
    AbcTenantInfo tenant,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext applicationDbContext)
{
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        await InitializeDefaultRolesAsync(cancellationToken);
        await InitializeAdminUserAsync();
    }

    private async Task InitializeDefaultRolesAsync(CancellationToken cancellationToken)
    {
        foreach (string roleName in RoleConstants.DefaultRoles)
        {
            ApplicationRole? incomingRole = await roleManager.FindByNameAsync(roleName);

            if (incomingRole is null)
            {
                incomingRole = new ApplicationRole()
                {
                    Name = roleName,
                    Description = $"{roleName} Role"
                };

                await roleManager.CreateAsync(incomingRole);
            }

            // assign permissions to newly added role
            switch (roleName)
            {
                case RoleConstants.Basic:
                    await AssignPermissionsToRole(SchoolPermissions.Basic, incomingRole,
                        cancellationToken);
                    break;
                case RoleConstants.Admin:
                    await AssignPermissionsToRole(SchoolPermissions.Admin, incomingRole,
                        cancellationToken);
                    break;
            }
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        if (string.IsNullOrEmpty(tenant.AdminEmail))
        {
            return;
        }

        ApplicationUser? adminUser = await userManager.Users.FirstOrDefaultAsync(x => x.Email == tenant.AdminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser()
            {
                FirstName = TenancyConstants.FirstName,
                LastName = TenancyConstants.LastName,
                Email = tenant.AdminEmail,
                UserName = tenant.AdminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = tenant.AdminEmail.ToUpper(),
                NormalizedUserName = tenant.AdminEmail.ToUpper(),
                IsActive = true
            };

            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, TenancyConstants.DefaultPassword);

            await userManager.CreateAsync(adminUser);
        }

        // assign user to admin role
        if (!await userManager.IsInRoleAsync(adminUser, RoleConstants.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
        }
    }

    private async Task AssignPermissionsToRole(
        IReadOnlyCollection<SchoolPermission> permissions, ApplicationRole role, CancellationToken cancellationToken)
    {
        IList<Claim> currentClaims = await roleManager.GetClaimsAsync(role);

        foreach (SchoolPermission rolePermission in permissions)
        {
            if (currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == rolePermission.Name))
            {
                continue;
            }

            await applicationDbContext.RoleClaims.AddAsync(new IdentityRoleClaim<string>
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = rolePermission.Name
            }, cancellationToken);

            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
