namespace Infrastructure.Persistence.DbInitializers;

public interface ITenantDbInitializer
{
    Task InitializeDatabaseAsync(CancellationToken cancellationToken);
}
