namespace Infrastructure.Identity;

using Application.Exceptions;
using Application.Features.Identity.Users;
using Constants;
using Finbuckle.MultiTenant;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Persistence.Contexts;
using Tenancy;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext applicationDbContext,
    ITenantInfo tenant)
    : IUserService
{
    public async Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ConflictException("Passwords do not match");
        }

        if (await IsEmailTakenAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("Email is already taken");
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive
        };

        IdentityResult result = await userManager.CreateAsync(user, request.Password);

        return !result.Succeeded
            ? throw new IdentityException("Failed to create user.", GetIdentityResultErrorDescription(result))
            : user.Id;
    }

    public async Task<string> UpdateUserAsync(UpdateUserRequest request)
    {
        ApplicationUser user = await userManager.FindByIdAsync(request.Id) ??
                               throw new NotFoundException($"User {request.Id} not found");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        IdentityResult result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new IdentityException("Failed to update user.", GetIdentityResultErrorDescription(result));
        }

        await signInManager.RefreshSignInAsync(user);

        return user.Id;
    }

    public async Task<string> DeleteUserAsync(string userId)
    {
        ApplicationUser user = await userManager.FindByIdAsync(userId) ??
                               throw new NotFoundException($"User {userId} not found");

        await userManager.DeleteAsync(user);

        await applicationDbContext.SaveChangesAsync();

        return user.Id;
    }

    public async Task<string> ActivateOrDeactivateAsync(string userId, bool activation)
    {
        ApplicationUser user = await GetUserAsync(userId);

        user.IsActive = activation;

        await userManager.UpdateAsync(user);

        return user.Id;
    }

    public async Task<string> ChangePasswordAsync(ChangePasswordRequest request)
    {
        ApplicationUser user = await GetUserAsync(request.UserId);

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new ConflictException("Passwords do not match");
        }

        IdentityResult result =
            await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return !result.Succeeded
            ? throw new IdentityException("Failed to change password.", GetIdentityResultErrorDescription(result))
            : user.Id;
    }

    public async Task<string> AssignRolesAsync(string userId, UserRolesRequest request)
    {
        ApplicationUser user = await GetUserAsync(userId);

        // Check if the user is an admin and if the Admin role is being removed
        bool isAdmin = await userManager.IsInRoleAsync(user, RoleConstants.Admin);
        bool isRemovingAdminRole = request.UserRoles.Any(ur => ur is { IsAssigned: false, Name: RoleConstants.Admin });

        if (isAdmin && isRemovingAdminRole)
        {
            int adminUsersCount = (await userManager.GetUsersInRoleAsync(RoleConstants.Admin)).Count;

            // Prevent removing Admin role from a Root tenant user
            bool isRootUser = user.Email == TenancyConstants.Root.Email;
            bool isRootTenant = tenant.Id == TenancyConstants.Root.Id;

            if (isRootUser && isRootTenant)
            {
                throw new ConflictException("Not allowed to remove Admin Role for a Root tenant user");
            }

            // Ensure there are at least 2 admin users in the tenant
            if (adminUsersCount <= 2)
            {
                throw new ConflictException("Tenant should have at least 2 admin users");
            }
        }

        // Filter roles that exist by asynchronously checking with roleManager
        var existingRoles = await Task.WhenAll(
            request.UserRoles.Select(async role => new
            {
                Role = role,
                Exists = await roleManager.FindByIdAsync(role.RoleId) != null
            }));

        // Process each existing role
        await Task.WhenAll(existingRoles
            .Where(r => r.Exists)
            .Select(async r =>
            {
                if (r.Role.IsAssigned && !string.IsNullOrWhiteSpace(r.Role.Name))
                {
                    bool isInRole = await userManager.IsInRoleAsync(user, r.Role.Name);

                    if (!isInRole)
                    {
                        await userManager.AddToRoleAsync(user, r.Role.Name);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(r.Role.Name))
                {
                    await userManager.RemoveFromRoleAsync(user, r.Role.Name);
                }
            }));

        return user.Id;
    }

    public async Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        List<ApplicationUser> users = await userManager.Users.AsNoTracking().ToListAsync(cancellationToken);

        return users.Adapt<List<UserDto>>();
    }

    public async Task<UserDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        ApplicationUser user = await userManager.FindByIdAsync(userId) ??
                               throw new NotFoundException($"User {userId} not found");

        return user.Adapt<UserDto>();
    }

    public async Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken cancellationToken)
    {
        var userRoles = new List<UserRoleDto>();

        ApplicationUser user = await userManager.FindByIdAsync(userId) ??
                               throw new NotFoundException($"User {userId} not found");

        List<ApplicationRole> roles = await roleManager
            .Roles.AsNoTracking().ToListAsync(cancellationToken);

        foreach (ApplicationRole role in roles)
        {
            bool isAssigned = !string.IsNullOrEmpty(role.Name) && await userManager.IsInRoleAsync(user, role.Name);

            userRoles.Add(new UserRoleDto
            {
                RoleId = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsAssigned = isAssigned
            });
        }

        return userRoles;
    }

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken)
    {
        ApplicationUser user = await userManager.FindByIdAsync(userId) ??
                               throw new NotFoundException($"User {userId} not found");

        IList<string> roles = await userManager.GetRolesAsync(user);

        var permissions = new List<string>();

        foreach (ApplicationRole role in await roleManager.Roles
                     .Where(r => !string.IsNullOrEmpty(r.Name) && roles.Contains(r.Name))
                     .ToListAsync(cancellationToken))
        {
            List<string> claims = await applicationDbContext.RoleClaims
                .Where(r => r.RoleId == role.Id && r.ClaimType == ClaimConstants.Permission)
                .Select(r => r.ClaimValue!)
                .ToListAsync(cancellationToken);

            permissions.AddRange(claims);
        }

        return permissions.Distinct().ToList();
    }

    public async Task<bool>
        IsPermissionAssignedAsync(string userId, string permission, CancellationToken cancellationToken = default) =>
        (await GetPermissionsAsync(userId, cancellationToken)).Contains(permission);

    private static List<string> GetIdentityResultErrorDescription(IdentityResult result) =>
        result.Errors.Select(error => error.Description).ToList();

    private async Task<ApplicationUser> GetUserAsync(string userId) =>
        await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User does not exist");
}
