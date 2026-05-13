using SmartInventory.Application.Asset.Dictionaries;
using SmartInventory.Application.Asset.DTOs;

namespace SmartInventory.Application.Asset.Services;

public class CategoryService
{
    public List<CategoryDto> GetAllCategories()
    {
        return CategoryGroupDictionary.AllCategories
            .Select(c => new CategoryDto
            {
                Name = c,
                Group = CategoryGroupDictionary.GetGroup(c) ?? string.Empty
            })
            .ToList();
    }

    public List<CategoryGroupDto> GetAllGroups()
    {
        return CategoryGroupDictionary.AllGroups
            .Select(g => new CategoryGroupDto
            {
                Group = g,
                Categories = CategoryGroupDictionary.GetCategoriesInGroup(g) ?? new List<string>()
            })
            .ToList();
    }

    public string? GetGroupForCategory(string category)
    {
        return CategoryGroupDictionary.GetGroup(category);
    }

    public bool IsValidCategory(string category)
    {
        return CategoryGroupDictionary.IsValidCategory(category);
    }

    public bool IsValidGroup(string group)
    {
        return CategoryGroupDictionary.IsValidGroup(group);
    }

    public List<string> GetCategoriesInGroup(string group)
    {
        return CategoryGroupDictionary.GetCategoriesInGroup(group) ?? new List<string>();
    }
}