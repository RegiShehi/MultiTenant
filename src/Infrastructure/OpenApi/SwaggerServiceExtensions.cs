namespace Infrastructure.OpenApi;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag.Generation.Processors.Security;

internal static class SwaggerServiceExtensions
{
    internal static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        SwaggerSettings? swaggerConfig = configuration.GetSection("SwaggerSettings").Get<SwaggerSettings>();

        services.AddEndpointsApiExplorer();
        _ = services.AddOpenApiDocument((document, _) =>
        {
            document.PostProcess = apiDocument =>
            {
                apiDocument.Info.Title = swaggerConfig?.Title;
                apiDocument.Info.Description = swaggerConfig?.Description;
                apiDocument.Info.Contact = new NSwag.OpenApiContact
                {
                    Name = swaggerConfig?.ContactName ?? "Test Name",
                    Email = swaggerConfig?.ContactEmail ?? "Test Email",
                    Url = swaggerConfig?.ContactUrl is not null ? swaggerConfig.ContactUrl.ToString() : string.Empty
                };
                apiDocument.Info.License = new NSwag.OpenApiLicense
                {
                    Name = swaggerConfig?.LicenseName,
                    Url = swaggerConfig?.LicenseUrl is not null ? swaggerConfig.LicenseUrl.ToString() : string.Empty
                };
            };

            document.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new NSwag.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your JWT bearer token to access this API",
                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor());
            document.OperationProcessors.Add(new SwaggerGlobalAuthProcessor());
            document.OperationProcessors.Add(new SwaggerHeaderAttributeProcessor());
        });

        return services;
    }

    internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi(options =>
        {
            options.DefaultModelExpandDepth = -1;
            options.DocExpansion = "none";
            options.TagsSorter = "alpha";
        });

        return app;
    }
}
