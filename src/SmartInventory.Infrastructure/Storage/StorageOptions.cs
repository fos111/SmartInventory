namespace SmartInventory.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
}
