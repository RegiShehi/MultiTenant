using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenancy;

public class TenantDbContext(DbContextOptions<TenantDbContext> options) : EFCoreStoreDbContext<AbcTenantInfo>(options)
{
}