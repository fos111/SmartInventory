using System.Threading;
using System.Threading.Tasks;

namespace SmartInventory.IoTLocation.Interfaces;

public interface IIoTLocationService
{
    Task<LocationProcessingResult> ProcessLocationAsync(string jsonPayload, CancellationToken cancellationToken = default);
}

public class LocationProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AssetId { get; set; }
    public string? RoomCode { get; set; }
}
