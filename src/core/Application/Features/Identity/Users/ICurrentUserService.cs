namespace Application.Features.Identity.Users;

using System.Security.Claims;

public interface ICurrentUserService
{
    string? Name { get; }

    string GetUserId();

    string GetUserEmail();

    string GetUserTenant();

    bool IsAuthenticated();

    bool IsInRole(string role);

    IEnumerable<Claim> GetUserClaims();

    void SetCurrentUser(ClaimsPrincipal? claimsPrincipal);
}
