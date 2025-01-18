using Application.Features.Tenancy;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Http;
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
            .WithClaimStrategy(ClaimConstants.Tenant)
            .WithCustomQueryStringStrategy(TenancyConstants.TenantIdName)
            .WithEFCoreStore<TenantDbContext, AbcTenantInfo>().Services
            .AddScoped<ITenantService, TenantService>();
    }

    private static FinbuckleMultiTenantBuilder<AbcTenantInfo> WithCustomQueryStringStrategy(
        this FinbuckleMultiTenantBuilder<AbcTenantInfo> builder, string customQueryStringStrategy)
    {
        return builder
            .WithDelegateStrategy(context =>
            {
                if (context is not HttpContext httpContext) return Task.FromResult<string>(null!)!;

                httpContext.Request.Query.TryGetValue(customQueryStringStrategy, out var tenantIdParam);

                return Task.FromResult(tenantIdParam.ToString())!;
            });
    }
}