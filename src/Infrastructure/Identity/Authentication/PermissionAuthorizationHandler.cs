namespace Infrastructure.Identity.Authentication;

using Application.Features.Identity.Users;
using Microsoft.AspNetCore.Authorization;

public class PermissionAuthorizationHandler(IUserService userService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        string userId = context.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        bool permissionsAssigned = await userService.IsPermissionAssignedAsync(userId, requirement.Permission);

        if (permissionsAssigned)
        {
            context.Succeed(requirement);
        }
    }
}
