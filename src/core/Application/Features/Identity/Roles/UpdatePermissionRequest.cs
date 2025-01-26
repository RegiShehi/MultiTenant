namespace Application.Features.Identity.Roles;

public class UpdatePermissionRequest
{
    public required string RoleId { get; set; }
    public required List<string> Permissions { get; set; }
}
