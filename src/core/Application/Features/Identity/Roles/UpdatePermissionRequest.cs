namespace Application.Features.Identity.Roles;

public class UpdatePermissionRequest
{
    private readonly List<string> permissions = [];

    public required string RoleId { get; set; }

    public IReadOnlyCollection<string> Permissions => permissions.AsReadOnly();

    // Method to modify the permissions safely
    public void RemovePermissionsStartingWith(
        string prefix,
        StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        permissions.RemoveAll(p => p.StartsWith(prefix, comparisonType));
    }
}
