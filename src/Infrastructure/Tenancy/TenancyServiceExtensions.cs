using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy;

internal static class TenancyServiceExtensions
{
    internal static IServiceCollection AddMultiTenancyServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddDbContext<TenantDbContext>(options => options
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
            .AddMultiTenant<AbcTenantInfo>()
            .WithHeaderStrategy(TenancyConstants.TenantIdName)
            .WithEFCoreStore<TenantDbContext, AbcTenantInfo>()
            .Services;
    }
}