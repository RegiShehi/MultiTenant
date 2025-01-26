namespace Infrastructure.Persistence.DbInitializers;

using System.Security.Claims;
using Identity.Constants;
using Identity.Models;
using Contexts;
using Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
                    await AssignPermissionsToRole(SchoolPermissions.Basic, incomingRole, cancellationToken);
                    break;
                case RoleConstants.Admin:
                    await AssignPermissionsToRole(SchoolPermissions.Admin, incomingRole, cancellationToken);
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
                NormalizedEmail = tenant.AdminEmail.ToUpperInvariant(),
                NormalizedUserName = tenant.AdminEmail.ToUpper(System.Globalization.CultureInfo.CurrentCulture),
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
        IReadOnlyCollection<SchoolPermissionDetails> permissions,
        ApplicationRole role,
        CancellationToken cancellationToken)
    {
        IList<Claim> currentClaims = await roleManager.GetClaimsAsync(role);

        // Identify permissions that are not present in currentClaims
        var newClaims = permissions
            .Where(p => !currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == p.Name))
            .Select(p => new IdentityRoleClaim<string>
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = p.Name
            })
            .ToList();

        if (newClaims.Count > 0)
        {
            await applicationDbContext.RoleClaims.AddRangeAsync(newClaims, cancellationToken);
            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
