namespace Infrastructure.Identity;

using Application.Features.Identity.Roles;
using Application.Features.Identity.Tokens;
using Models;
using Tokens;
using Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

internal static class IdentityServiceExtensions
{
    internal static IServiceCollection AddIdentityServices(this IServiceCollection services) =>
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services
            .AddTransient<ITokenService, TokenService>()
            .AddTransient<IRoleService, RoleService>();
}
