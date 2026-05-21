using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartInventory.IoTLocation.Interfaces;

namespace SmartInventory.IoTLocation;

public class Worker : BackgroundService
{
    private readonly IMqttClientWrapper _mqttClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IMqttClientWrapper mqttClient, IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
    {
        _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
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

    private Task ProcessMessageAsync(MqttMessageReceivedEventArgs args)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var locationService = scope.ServiceProvider.GetRequiredService<IIoTLocationService>();
                var result = await locationService.ProcessLocationAsync(args.Payload, CancellationToken.None);

                if (result.Success)
                {
                    _logger.LogInformation("Processed location for asset {AssetId}, room {RoomCode}", 
                        result.AssetId, result.RoomCode);
                }
                else
                {
                    _logger.LogWarning("Failed to process location: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message in background task");
            }
        });

        return Task.CompletedTask;
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
