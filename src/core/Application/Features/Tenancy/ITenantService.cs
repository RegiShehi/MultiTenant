using Application.Features.Tenancy.Commands;

namespace Application.Features.Tenancy;

public interface ITenantService
{
    Task<string> CreateTenantAsync(CreateTenantRequest createTenantRequest);
}