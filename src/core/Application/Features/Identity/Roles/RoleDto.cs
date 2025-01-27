namespace Application.Features.Identity.Roles;

public class RoleDto
{
    private readonly List<string> permissions = [];

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string Id { get; set; }

    public IReadOnlyCollection<string> Permissions => permissions.AsReadOnly();

    public void SetPermissions(IReadOnlyList<string> newPermissions)
    {
        permissions.Clear();
        permissions.AddRange(newPermissions);
    }
}
