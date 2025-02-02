namespace Infrastructure.OpenApi;

using System.Reflection;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

public class SwaggerHeaderAttributeProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        SwaggerHeaderAttribute? swaggerHeader = context.MethodInfo.GetCustomAttribute<SwaggerHeaderAttribute>();

        if (swaggerHeader is null)
        {
            return true;
        }

        IList<OpenApiParameter>? parameters = context.OperationDescription.Operation.Parameters;

        OpenApiParameter? existingParam = parameters
            .FirstOrDefault(p => p.Kind == OpenApiParameterKind.Header && p.Name == swaggerHeader.HeaderName);

        if (existingParam is not null)
        {
            parameters.Remove(existingParam);
        }

        parameters.Add(new OpenApiParameter
        {
            Name = swaggerHeader.HeaderName,
            Kind = OpenApiParameterKind.Header,
            Description = swaggerHeader.Description,
            IsRequired = swaggerHeader.IsRequired,
            Schema = new NJsonSchema.JsonSchema
            {
                Type = NJsonSchema.JsonObjectType.String,
                Default = swaggerHeader.DefaultValue
            }
        });

        return true;
    }
}
