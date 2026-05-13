using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartInventory.IoTLocation.Interfaces;

namespace SmartInventory.IoTLocation;

public class Worker : BackgroundService
{
    private readonly IMqttClientWrapper _mqttClient;
    private readonly IIoTLocationService _locationService;
    private readonly ILogger<Worker> _logger;

    public Worker(IMqttClientWrapper mqttClient, IIoTLocationService locationService, ILogger<Worker> logger)
    {
        _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _mqttClient.MessageReceived += ProcessMessageAsync;

        try
        {
            await _mqttClient.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start MQTT client");
            throw;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(MqttMessageReceivedEventArgs args)
    {
        try
        {
            _ = Task.Run(async () =>
            {
                var result = await _locationService.ProcessLocationAsync(args.Payload, CancellationToken.None);
                
                if (result.Success)
                {
                    _logger.LogInformation("Processed location for asset {AssetId}, room {RoomCode}", 
                        result.AssetId, result.RoomCode);
                }
                else
                {
                    _logger.LogWarning("Failed to process location: {Error}", result.ErrorMessage);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _mqttClient.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping MQTT client");
        }

        await base.StopAsync(cancellationToken);
    }
}
