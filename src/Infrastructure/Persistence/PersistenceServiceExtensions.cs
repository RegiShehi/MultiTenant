namespace Infrastructure.Persistence;

using Contexts;
using DbInitializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddDbContext<ApplicationDbContext>(options => options
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
            .AddTransient<ITenantDbInitializer, TenantDbInitializer>()
            .AddTransient<ApplicationDbInitializer>();

    public static async Task AddDatabaseInitializerAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ITenantDbInitializer>()
            .InitializeDatabaseAsync(cancellationToken);
    }
}
