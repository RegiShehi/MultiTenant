namespace Infrastructure.Identity.Authentication;

using Application.Features.Identity.Users;
using Microsoft.AspNetCore.Http;

public class CurrentUserMiddleware(ICurrentUserService currentUserService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        currentUserService.SetCurrentUser(context.User);

        await next(context);
    }
}
