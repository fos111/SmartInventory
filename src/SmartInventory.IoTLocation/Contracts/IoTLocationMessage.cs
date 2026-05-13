namespace SmartInventory.IoTLocation.Contracts;

public class IoTLocationMessage
{
    public Guid AssetId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
