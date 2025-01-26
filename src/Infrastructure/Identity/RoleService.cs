namespace Infrastructure.Identity;

using Application.Features.Identity.Roles;

public class RoleService : IRoleService
{
    public Task<string> CreateAsync(CreateRoleRequest request) => throw new NotImplementedException();

    public Task<string> UpdateAsync(UpdateRoleRequest request) => throw new NotImplementedException();

    public Task<string> DeleteAsync(string id) => throw new NotImplementedException();

    public Task<string> UpdatePermissionsAsync(UpdatePermissionRequest request) => throw new NotImplementedException();

    public Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<RoleDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<bool> DoesItExistAsync(string name, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
