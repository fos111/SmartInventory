using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Mobile.Auth.Helpers;

public static class MobileRoleMapper
{
    private static readonly Dictionary<string, UserRole> MobileToDotNet = new(StringComparer.OrdinalIgnoreCase)
    {
        { "technicien", UserRole.Technicien },
        { "magazinier", UserRole.Gestionnaire },
        { "admin", UserRole.Supervisor },
    };

    private static readonly Dictionary<UserRole, string> DotNetToMobile = new()
    {
        { UserRole.Technicien, "technicien" },
        { UserRole.Gestionnaire, "magazinier" },
        { UserRole.Supervisor, "admin" },
    };

    public static UserRole MapToDotNet(string mobileRole)
    {
        if (MobileToDotNet.TryGetValue(mobileRole, out var role))
            return role;

        throw new ArgumentException($"Unknown mobile role: {mobileRole}. Allowed: technicien, magazinier, admin");
    }

    public static string MapToMobile(UserRole role)
    {
        if (DotNetToMobile.TryGetValue(role, out var mobileRole))
            return mobileRole;

        throw new ArgumentException($"Unknown .NET role: {role}");
    }

    public static bool IsValidMobileRole(string mobileRole)
    {
        return MobileToDotNet.ContainsKey(mobileRole);
    }

    public static string DefaultMobileRole => "technicien";
}
