namespace Infrastructure.OpenApi;

using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Namotion.Reflection;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

public class SwaggerGlobalAuthProcessor(string scheme) : IOperationProcessor
{
    public SwaggerGlobalAuthProcessor()
        : this(JwtBearerDefaults.AuthenticationScheme)
    {
    }

    public bool Process(OperationProcessorContext context)
    {
        IList<object>? list = ((AspNetCoreOperationProcessorContext)context)
            .ApiDescription.ActionDescriptor.TryGetPropertyValue<IList<object>>("EndpointMetadata");

        if (list is null)
        {
            return true;
        }

        if (list.OfType<AllowAnonymousAttribute>().Any())
        {
            return true;
        }

        // Ensure the Security collection is initialized.
        context.OperationDescription.Operation.Security ??= [];

        // If the Security collection is empty, add the new requirement.
        if (context.OperationDescription.Operation.Security.Count == 0)
        {
            context.OperationDescription.Operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    scheme,
                    Array.Empty<string>()
                }
            });
        }

        return true;
    }
}
