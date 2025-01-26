namespace Application.Features.Tenancy;

using Commands;
using Models;

public interface ITenantService
{
    Task<string> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken);

    Task<string> ActivateAsync(string id);

    Task<string> DeactivateAsync(string id);

    Task<string> UpdateSubscriptionAsync(string id, DateTime newExpiryDate);

    Task<List<TenantDto>> GetTenantsAsync();

    Task<TenantDto> GetTenantByIdAsync(string id);
}
