namespace Application.Features.Identity.Users;

public class UserRoleDto
{
    public required string RoleId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsAssigned { get; set; }
}

public class UserRolesRequest
{
    private readonly List<UserRoleDto> userRoles = [];

    public IReadOnlyCollection<UserRoleDto> UserRoles => userRoles.AsReadOnly();
}
