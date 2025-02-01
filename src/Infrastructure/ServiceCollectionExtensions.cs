namespace Infrastructure;

using Identity;
using Microsoft.AspNetCore.Builder;
using Persistence;
using Tenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddMultiTenancyServices(configuration)
            .AddPersistenceServices(configuration)
            .AddIdentityServices()
            .AddPermissions();

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app) => app.AddCurrentUser();
}
