using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartInventory.IoTLocation.Interfaces;

public interface IMqttClientWrapper
{
    event Func<MqttMessageReceivedEventArgs, Task> MessageReceived;
    
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public class MqttMessageReceivedEventArgs : EventArgs
{
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
