namespace Application.Features.Identity.Roles;

public class UpdatePermissionRequest
{
    public required string RoleId { get; set; }
    public required IReadOnlyList<string> Permissions { get; set; }
}
