using Application.Features.Tenancy;
using Application.Features.Tenancy.Commands;

namespace Infrastructure.Tenancy;

internal class TenantService : ITenantService
{
    public Task<string> CreateTenantAsync(CreateTenantRequest createTenantRequest)
    {
        throw new NotImplementedException();
    }
}