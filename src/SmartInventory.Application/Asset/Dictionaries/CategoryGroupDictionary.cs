namespace SmartInventory.Application.Asset.Dictionaries;

public static class CategoryGroupDictionary
{
    private static readonly Dictionary<string, string> _categoryToGroup = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Computer", "IT Equipment" },
        { "Server", "IT Equipment" },
        { "Network Device", "IT Equipment" },
        { "Peripheral", "IT Equipment" },
        { "Printer/Scanner", "IT Equipment" },
        { "Display", "AV Equipment" },
        { "Projector", "AV Equipment" },
        { "Machine Tool", "Lab Equipment" }
    };

    private static readonly Dictionary<string, List<string>> _groupToCategories = new()
    {
        { "IT Equipment", new List<string> { "Computer", "Server", "Network Device", "Peripheral", "Printer/Scanner" } },
        { "AV Equipment", new List<string> { "Display", "Projector" } },
        { "Lab Equipment", new List<string> { "Machine Tool" } }
    };

    private static readonly List<string> _allCategories = new()
    {
        "Computer", "Server", "Network Device", "Peripheral", "Printer/Scanner", "Display", "Projector", "Machine Tool"
    };

    private static readonly List<string> _allGroups = new()
    {
        "IT Equipment", "AV Equipment", "Lab Equipment"
    };

    public static IReadOnlyDictionary<string, string> CategoryToGroup => _categoryToGroup;
    public static IReadOnlyDictionary<string, List<string>> GroupToCategories => _groupToCategories;
    public static IReadOnlyList<string> AllCategories => _allCategories;
    public static IReadOnlyList<string> AllGroups => _allGroups;

    public static string? GetGroup(string category)
    {
        return _categoryToGroup.TryGetValue(category, out var group) ? group : null;
    }

    public static List<string>? GetCategoriesInGroup(string group)
    {
        return _groupToCategories.TryGetValue(group, out var categories) ? categories : null;
    }

    public static bool IsValidCategory(string category)
    {
        return _allCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValidGroup(string group)
    {
        return _allGroups.Contains(group, StringComparer.OrdinalIgnoreCase);
    }
}