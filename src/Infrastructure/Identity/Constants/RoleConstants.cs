using System.Collections.ObjectModel;

namespace Infrastructure.Identity.Constants;

public class RoleConstants
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>([Admin, Basic]);

    public static bool IsDefault(string roleName)
    {
        return DefaultRoles.Contains(roleName);
    }
}