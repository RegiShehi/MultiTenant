namespace Application.Features.Identity.Roles;

public interface IRoleService
{
    Task<string> CreateAsync(CreateRoleRequest request);

    Task<string> UpdateAsync(UpdateRoleRequest request);

    Task<string> DeleteAsync(string id);

    Task<string> UpdatePermissionsAsync(UpdatePermissionRequest request);

    Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);

    Task<RoleDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken);

    Task<bool> DoesItExistAsync(string name, CancellationToken cancellationToken);
}
