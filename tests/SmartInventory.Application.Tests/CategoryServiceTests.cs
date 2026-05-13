using FluentAssertions;
using SmartInventory.Application.Asset.Dictionaries;
using SmartInventory.Application.Asset.Services;
using Xunit;

namespace SmartInventory.Application.Tests;

public class CategoryServiceTests : ApplicationTestBase
{
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService();
    }

    [Fact]
    public void GetAllCategories_ReturnsEightCategories()
    {
        var result = _service.GetAllCategories();

        result.Should().HaveCount(8);
    }

    [Fact]
    public void GetAllCategories_ContainsComputer()
    {
        var result = _service.GetAllCategories();

        result.Should().Contain(c => c.Name == "Computer");
    }

    [Fact]
    public void GetAllCategories_ComputerHasCorrectGroup()
    {
        var result = _service.GetAllCategories();

        var computer = result.Should().Contain(c => c.Name == "Computer").Subject;
        computer.Group.Should().Be("IT Equipment");
    }

    [Fact]
    public void GetAllCategories_ContainsAllValidCategories()
    {
        var result = _service.GetAllCategories();

        var categoryNames = result.Select(c => c.Name).ToList();
        categoryNames.Should().Contain("Computer");
        categoryNames.Should().Contain("Server");
        categoryNames.Should().Contain("Network Device");
        categoryNames.Should().Contain("Peripheral");
        categoryNames.Should().Contain("Printer/Scanner");
        categoryNames.Should().Contain("Display");
        categoryNames.Should().Contain("Projector");
        categoryNames.Should().Contain("Machine Tool");
    }

    [Fact]
    public void GetAllGroups_ReturnsThreeGroups()
    {
        var result = _service.GetAllGroups();

        result.Should().HaveCount(3);
    }

    [Fact]
    public void GetAllGroups_ITEquipmentHasFiveCategories()
    {
        var result = _service.GetAllGroups();

        var itGroup = result.Should().Contain(g => g.Group == "IT Equipment").Subject;
        itGroup.Categories.Should().HaveCount(5);
        itGroup.Categories.Should().Contain("Computer");
        itGroup.Categories.Should().Contain("Server");
    }

    [Fact]
    public void GetAllGroups_AVEquipmentHasTwoCategories()
    {
        var result = _service.GetAllGroups();

        var avGroup = result.Should().Contain(g => g.Group == "AV Equipment").Subject;
        avGroup.Categories.Should().HaveCount(2);
    }

    [Fact]
    public void GetAllGroups_LabEquipmentHasOneCategory()
    {
        var result = _service.GetAllGroups();

        var labGroup = result.Should().Contain(g => g.Group == "Lab Equipment").Subject;
        labGroup.Categories.Should().HaveCount(1);
        labGroup.Categories.Should().Contain("Machine Tool");
    }

    [Fact]
    public void GetGroupForCategory_Computer_ReturnsITEquipment()
    {
        var result = _service.GetGroupForCategory("Computer");

        result.Should().Be("IT Equipment");
    }

    [Fact]
    public void GetGroupForCategory_Projector_ReturnsAVEquipment()
    {
        var result = _service.GetGroupForCategory("Projector");

        result.Should().Be("AV Equipment");
    }

    [Fact]
    public void GetGroupForCategory_MachineTool_ReturnsLabEquipment()
    {
        var result = _service.GetGroupForCategory("Machine Tool");

        result.Should().Be("Lab Equipment");
    }

    [Fact]
    public void GetGroupForCategory_InvalidCategory_ReturnsNull()
    {
        var result = _service.GetGroupForCategory("InvalidCategory");

        result.Should().BeNull();
    }

    [Fact]
    public void IsValidCategory_Valid_ReturnsTrue()
    {
        _service.IsValidCategory("Computer").Should().BeTrue();
        _service.IsValidCategory("Server").Should().BeTrue();
        _service.IsValidCategory("Machine Tool").Should().BeTrue();
    }

    [Fact]
    public void IsValidCategory_Invalid_ReturnsFalse()
    {
        _service.IsValidCategory("InvalidCategory").Should().BeFalse();
        _service.IsValidCategory("").Should().BeFalse();
    }

    [Fact]
    public void IsValidGroup_Valid_ReturnsTrue()
    {
        _service.IsValidGroup("IT Equipment").Should().BeTrue();
        _service.IsValidGroup("AV Equipment").Should().BeTrue();
        _service.IsValidGroup("Lab Equipment").Should().BeTrue();
    }

    [Fact]
    public void IsValidGroup_Invalid_ReturnsFalse()
    {
        _service.IsValidGroup("InvalidGroup").Should().BeFalse();
    }

    [Fact]
    public void GetCategoriesInGroup_ITEquipment_ReturnsFiveCategories()
    {
        var result = _service.GetCategoriesInGroup("IT Equipment");

        result.Should().HaveCount(5);
        result.Should().Contain("Computer");
    }

    [Fact]
    public void GetCategoriesInGroup_InvalidGroup_ReturnsEmptyList()
    {
        var result = _service.GetCategoriesInGroup("InvalidGroup");

        result.Should().BeEmpty();
    }
}

public class CategoryGroupDictionaryTests : ApplicationTestBase
{
    [Fact]
    public void AllCategories_ContainsExactlyEight()
    {
        CategoryGroupDictionary.AllCategories.Should().HaveCount(8);
    }

    [Fact]
    public void AllGroups_ContainsExactlyThree()
    {
        CategoryGroupDictionary.AllGroups.Should().HaveCount(3);
    }

    [Fact]
    public void CategoryToGroup_MappingComplete()
    {
        foreach (var category in CategoryGroupDictionary.AllCategories)
        {
            var group = CategoryGroupDictionary.GetGroup(category);
            group.Should().NotBeNull();
        }
    }

    [Fact]
    public void GetGroup_CaseInsensitive()
    {
        CategoryGroupDictionary.GetGroup("computer").Should().Be("IT Equipment");
        CategoryGroupDictionary.GetGroup("COMPUTER").Should().Be("IT Equipment");
        CategoryGroupDictionary.GetGroup("Computer").Should().Be("IT Equipment");
    }
}