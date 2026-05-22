namespace SmartInventory.Infrastructure.Storage;

public class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public string? ConnectionString { get; set; }
}
