﻿namespace Infrastructure.Persistence.DbInitializers;

using Finbuckle.MultiTenant;
using Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class TenantDbInitializer(
    TenantDbContext tenantDbContext,
    IServiceProvider serviceProvider)
    : ITenantDbInitializer
{
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        await InitializeRootTenantAsync(cancellationToken);

        foreach (AbcTenantInfo tenant in await tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
        {
            await InitializeApplicationDatabaseAsync(tenant, cancellationToken);
        }
    }

    private async Task InitializeRootTenantAsync(CancellationToken cancellationToken)
    {
        // check if root tenant exists
        if (await tenantDbContext.TenantInfo.FindAsync([TenancyConstants.Root.Id], cancellationToken) is null)
        {
            // create root tenant
            var rootTenant = new AbcTenantInfo
            {
                Id = TenancyConstants.Root.Id,
                Identifier = TenancyConstants.Root.Name,
                Name = TenancyConstants.Root.Name,
                AdminEmail = TenancyConstants.Root.Email,
                IsActive = true,
                ValidUpTo = DateTime.UtcNow.AddYears(1)
            };

            await tenantDbContext.TenantInfo.AddAsync(rootTenant, cancellationToken);
            await tenantDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task InitializeApplicationDatabaseAsync(AbcTenantInfo tenant, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        serviceProvider.GetRequiredService<IMultiTenantContextAccessor>()
            .MultiTenantContext = new MultiTenantContext<AbcTenantInfo>
        {
            TenantInfo = tenant
        };

        await serviceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeDatabaseAsync(cancellationToken);
    }
}
