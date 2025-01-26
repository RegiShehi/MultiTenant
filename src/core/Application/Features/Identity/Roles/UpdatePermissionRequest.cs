namespace Application.Features.Identity.Roles;

public class UpdatePermissionRequest
{
    public string RoleId { get; set; }
    public List<string> Permissions { get; set; }
}
