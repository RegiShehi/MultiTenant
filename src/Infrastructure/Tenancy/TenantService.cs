using Application.Features.Tenancy;
using Application.Features.Tenancy.Commands;
using Application.Features.Tenancy.Models;
using Finbuckle.MultiTenant;
using Infrastructure.Persistence.DbInitializers;
using Mapster;

namespace Infrastructure.Tenancy;

internal class TenantService(
    ApplicationDbInitializer applicationDbInitializer,
    IMultiTenantStore<AbcTenantInfo> tenantStore)
    : ITenantService
{
    public async Task<string> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        // create tenant in database
        var newTenant = new AbcTenantInfo
        {
            Id = request.Identifier,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            AdminEmail = request.AdminEmail,
            ValidUpTo = request.ValidUpTo,
            IsActive = request.IsActive
        };

        await tenantStore.TryAddAsync(newTenant);

        // initialize tenant with users, users roles, roles and permissions
        try
        {
            await applicationDbInitializer.InitializeDatabaseAsync(cancellationToken);
        }
        catch (Exception)
        {
            await tenantStore.TryRemoveAsync(request.Identifier);
            throw;
        }

        return newTenant.Id;
    }

    public async Task<string> ActivateAsync(string id)
    {
        var tenant = await tenantStore.TryGetByIdentifierAsync(id);

        if (tenant is null)
            throw new ArgumentException($"Tenant with {id} could not be found");

        tenant.IsActive = true;

        var updateSuccess = await tenantStore.TryUpdateAsync(tenant);

        if (!updateSuccess || tenant.Id is null)
            throw new InvalidOperationException($"Failed to update tenant with ID '{id}'.");

        return tenant.Id;
    }

    public async Task<string> DeactivateAsync(string id)
    {
        var tenant = await tenantStore.TryGetByIdentifierAsync(id);

        if (tenant is null)
            throw new ArgumentException($"Tenant with {id} could not be found");

        tenant.IsActive = false;

        var updateSuccess = await tenantStore.TryUpdateAsync(tenant);

        if (!updateSuccess || tenant.Id is null)
            throw new InvalidOperationException($"Failed to update tenant with ID '{id}'.");

        return tenant.Id;
    }

    public async Task<string> UpdateSubscriptionAsync(string id, DateTime newExpiryDate)
    {
        var tenant = await tenantStore.TryGetByIdentifierAsync(id);

        if (tenant is null)
            throw new ArgumentException($"Tenant with {id} could not be found");

        tenant.ValidUpTo = newExpiryDate;

        var updateSuccess = await tenantStore.TryUpdateAsync(tenant);

        if (!updateSuccess || tenant.Id is null)
            throw new InvalidOperationException($"Failed to update tenant with ID '{id}'.");

        return tenant.Id;
    }

    public async Task<List<TenantDto>> GetTenantsAsync()
    {
        var tenant = await tenantStore.GetAllAsync();

        return tenant.Adapt<List<TenantDto>>();
    }

    public async Task<TenantDto> GetTenantByIdAsync(string id)
    {
        var tenant = await tenantStore.TryGetByIdentifierAsync(id);

        if (tenant is null)
            throw new ArgumentException($"Tenant with {id} could not be found");

        #region Manual mapping - option 1

        // return new TenantDto
        // {
        //     Id = tenant.Id,
        //     Name = tenant.Name,
        //     ConnectionString = tenant.ConnectionString,
        //     AdminEmail = tenant.AdminEmail,
        //     ValidUpTo = tenant.ValidUpTo,
        //     IsActive = tenant.IsActive
        // };

        #endregion

        return tenant.Adapt<TenantDto>();
    }
}