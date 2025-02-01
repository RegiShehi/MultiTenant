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
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Build the WebApplication
        WebApplication app = builder.Build();

        // Initialize the database
        await app.Services.AddDatabaseInitializerAsync();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.UseInfrastructure();

        // Run the application
        await app.RunAsync();
    }
}
