namespace Infrastructure.OpenApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class BaseHeaderAttribute(string headerName, string description, string defaultValue, bool isRequired)
    : Attribute
{
    public string HeaderName { get; internal set; } = headerName;

    public string Description { get; internal set; } = description;

    public string? DefaultValue { get; internal set; } = defaultValue;

    public bool IsRequired { get; internal set; } = isRequired;
}
