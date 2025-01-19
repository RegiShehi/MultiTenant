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
    UserManager<ApplicationUser> userManager)
{
    public async Task InitializeDatabaseAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        await InitializeDefaultRoles(context, cancellationToken);
        await InitializeAdminUserAsync();
    }

    private async Task InitializeDefaultRoles(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        foreach (var roleName in RoleConstants.DefaultRoles)
        {
            var incomingRole = await roleManager.FindByNameAsync(roleName);

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
                    await AssignPermissionsToRole(context, SchoolPermissions.Basic, incomingRole, cancellationToken);
                    break;
                case RoleConstants.Admin:
                    await AssignPermissionsToRole(context, SchoolPermissions.Admin, incomingRole, cancellationToken);
                    break;
            }
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        if (string.IsNullOrEmpty(tenant.AdminEmail)) return;

        var adminUser = await userManager.Users.FirstOrDefaultAsync(x => x.Email == tenant.AdminEmail);

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
            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
    }

    private async Task AssignPermissionsToRole(ApplicationDbContext context,
        IReadOnlyCollection<SchoolPermission> permissions, ApplicationRole role, CancellationToken cancellationToken)
    {
        var currentClaims = await roleManager.GetClaimsAsync(role);

        foreach (var rolePermission in permissions)
        {
            if (currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == rolePermission.Name))
                continue;

            await context.RoleClaims.AddAsync(new IdentityRoleClaim<string>
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = rolePermission.Name
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}