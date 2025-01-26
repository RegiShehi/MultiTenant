namespace Infrastructure.Identity.Constants;

public static class SchoolAction
{
    public const string View = nameof(View);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class SchoolFeature
{
    public const string Tenants = nameof(Tenants);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Schools = nameof(Schools);
}

public record SchoolPermission(
    string Description,
    string Action,
    string Feature,
    bool IsBasic = false,
    bool IsRoot = false)
{
    public string Name => NameFor(Action, Feature);

    // Permission.Feature.Action
    private static string NameFor(string action, string feature) => $"Permission.{feature}.{action}";
}

public static class SchoolPermissions
{
    private static readonly SchoolPermission[] _allPermissions =
    [
        new("View Users", SchoolAction.View, SchoolFeature.Users),
        new("Create Users", SchoolAction.Create, SchoolFeature.Users),
        new("Update Users", SchoolAction.Update, SchoolFeature.Users),
        new("Delete Users", SchoolAction.Delete, SchoolFeature.Users),

        new("View User Roles", SchoolAction.View, SchoolFeature.UserRoles),
        new("Update User Roles", SchoolAction.Update, SchoolFeature.UserRoles),

        new("View Roles", SchoolAction.View, SchoolFeature.Roles),
        new("Create Roles", SchoolAction.Create, SchoolFeature.Roles),
        new("Update Roles", SchoolAction.Update, SchoolFeature.Roles),

        new("Delete Roles", SchoolAction.Delete, SchoolFeature.Roles),

        new("View Role Claims/Permissions", SchoolAction.View, SchoolFeature.RoleClaims),
        new("Update Role Claims/Permissions", SchoolAction.Update, SchoolFeature.RoleClaims),

        new("View Schools", SchoolAction.View, SchoolFeature.Schools, true),
        new("Create Schools", SchoolAction.Create, SchoolFeature.Schools),
        new("Update Schools", SchoolAction.Update, SchoolFeature.Schools),
        new("Delete Schools", SchoolAction.Delete, SchoolFeature.Schools),

        new("View Tenants", SchoolAction.View, SchoolFeature.Tenants, IsRoot: true),
        new("Create Tenants", SchoolAction.Create, SchoolFeature.Tenants, IsRoot: true),
        new("Update Tenants", SchoolAction.Update, SchoolFeature.Tenants, IsRoot: true),
        new("Upgrade Tenants Subscription", SchoolAction.UpgradeSubscription, SchoolFeature.Tenants, IsRoot: true)
    ];

    public static IReadOnlyCollection<SchoolPermission> All => _allPermissions;
    public static IReadOnlyCollection<SchoolPermission> Root => _allPermissions.Where(x => x.IsRoot).ToList();
    public static IReadOnlyCollection<SchoolPermission> Admin => _allPermissions.Where(x => !x.IsRoot).ToList();
    public static IReadOnlyCollection<SchoolPermission> Basic => _allPermissions.Where(x => x.IsBasic).ToList();
}
