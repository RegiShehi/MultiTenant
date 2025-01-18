using Finbuckle.MultiTenant.Stores;
using Infrastructure.Persistence.DbConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenancy;

public class TenantDbContext(DbContextOptions<TenantDbContext> options) : EFCoreStoreDbContext<AbcTenantInfo>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AbcTenantInfo>(entity =>
        {
            // Map to the "Tenants" table in the specified schema
            entity.ToTable("Tenants", SchemaNames.Multitenancy);

            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Identifier).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AdminEmail).IsRequired().HasMaxLength(100);
        });
    }
}