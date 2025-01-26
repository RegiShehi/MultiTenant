namespace Application.Features.Identity.Roles;

public class RoleDto
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string Id { get; set; }
}
