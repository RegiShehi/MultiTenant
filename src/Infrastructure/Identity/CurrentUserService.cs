namespace Infrastructure.Identity;

using System.Security.Claims;
using Application.Exceptions;
using Application.Features.Identity.Users;

public class CurrentUserService : ICurrentUserService
{
    private ClaimsPrincipal? _principal;

    public string? Name => _principal?.Identity?.Name;

    public string GetUserId() =>
        IsAuthenticated() && _principal is not null ? _principal.GetUserId() : string.Empty;

    public string GetUserEmail() =>
        IsAuthenticated() && _principal is not null ? _principal.GetEmail() : string.Empty;

    public string GetUserTenant() =>
        IsAuthenticated() && _principal is not null ? _principal.GetTenant() : string.Empty;

    public bool IsAuthenticated() => _principal?.Identity is { IsAuthenticated: true };

    public bool IsInRole(string role) => _principal is not null && _principal.IsInRole(role);

    public IEnumerable<Claim> GetUserClaims() => _principal is not null ? _principal.Claims : [];

    public void SetCurrentUser(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal is not null)
        {
            _principal = claimsPrincipal;
        }

        throw new ConflictException("Invalid operation on claim");
    }
}
