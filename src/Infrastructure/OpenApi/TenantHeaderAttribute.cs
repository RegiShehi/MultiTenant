namespace Infrastructure.OpenApi;

using Tenancy;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TenantHeaderAttribute : SwaggerHeaderAttribute
{
    public TenantHeaderAttribute()
        : base(TenancyConstants.TenantIdName, "Input tenant name to access API", string.Empty, true)
    {
    }
}
