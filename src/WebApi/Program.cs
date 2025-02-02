namespace WebApi;

using Infrastructure;
using Infrastructure.Persistence;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        // Create the WebApplication builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Build the WebApplication
        WebApplication app = builder.Build();

        // Initialize the database
        await app.Services.AddDatabaseInitializerAsync();

        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.UseInfrastructure();

        // Run the application
        await app.RunAsync();
    }
}
