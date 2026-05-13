using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using SmartInventory.IoTLocation.Interfaces;

namespace SmartInventory.IoTLocation.Infrastructure;

public class MqttClientWrapper : IMqttClientWrapper, IDisposable
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttSettings _settings;
    private readonly ILogger<MqttClientWrapper> _logger;
    private int _consecutiveFailures;
    private bool _disposed;

    public event Func<MqttMessageReceivedEventArgs, Task>? MessageReceived;

    public MqttClientWrapper(IOptions<MqttSettings> options, ILogger<MqttClientWrapper> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        _mqttClient.ConnectedAsync += OnConnected;
        _mqttClient.DisconnectedAsync += OnDisconnected;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Host, _settings.Port)
            .WithClientId($"{_settings.ClientIdPrefix}-{Guid.NewGuid()}")
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_settings.KeepAliveSeconds))
            .WithTimeout(TimeSpan.FromSeconds(_settings.TimeoutSeconds))
            .WithCleanStart(_settings.CleanStart)
            .Build();

        await ConnectWithRetryAsync(options, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        try
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(
                    new MqttClientDisconnectOptions { ReasonString = "Service stopping" },
                    cancellationToken);
                _logger.LogInformation("MQTT client disconnected gracefully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disconnecting MQTT client");
        }
    }

    private async Task ConnectWithRetryAsync(MqttClientOptions options, CancellationToken cancellationToken)
    {
        int attempt = 0;
        TimeSpan delay = TimeSpan.FromSeconds(1);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _mqttClient.ConnectAsync(options, cancellationToken);
                _consecutiveFailures = 0;
                return;
            }
            catch (Exception ex)
            {
                attempt++;
                _consecutiveFailures++;

                if (_consecutiveFailures >= 10)
                {
                    _logger.LogCritical(ex, "MQTT connection failed 10 consecutive times. Continuing to retry...");
                }
                else
                {
                    _logger.LogWarning(ex, "MQTT connection attempt {Attempt} failed. Retrying in {Delay}ms", attempt, delay.TotalMilliseconds);
                }

                await Task.Delay(delay, cancellationToken);

                // Exponential backoff: 1s, 2s, 4s, 8s, 16s, then 30s max
                delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, attempt)));
            }
        }
    }

    private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var payloadBytes = args.ApplicationMessage.Payload.ToArray();
            var payload = System.Text.Encoding.UTF8.GetString(payloadBytes);
            _logger.LogInformation("Message received on topic {Topic}: {Payload}", args.ApplicationMessage.Topic, payload);

            if (MessageReceived != null)
            {
                var eventArgs = new MqttMessageReceivedEventArgs
                {
                    Topic = args.ApplicationMessage.Topic,
                    Payload = payload,
                    ReceivedAt = DateTime.UtcNow
                };

                await MessageReceived.Invoke(eventArgs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message");
        }
    }

    private Task OnConnected(MqttClientConnectedEventArgs args)
    {
        _logger.LogInformation("MQTT client connected to {Host}:{Port}", _settings.Host, _settings.Port);
        return SubscribeToTopicAsync();
    }

    private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("MQTT client disconnected. Reason: {Reason}", args.Reason);

        if (args.ClientWasConnected)
        {
            _logger.LogInformation("Attempting to reconnect...");
        }

        return Task.CompletedTask;
    }

    private async Task SubscribeToTopicAsync()
    {
        try
        {
            var subscribeOptions = new MqttClientFactory()
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f
                    .WithTopic(_settings.Topic)
                    .WithAtLeastOnceQoS())
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);
            _logger.LogInformation("Subscribed to topic: {Topic}", _settings.Topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topic {Topic}", _settings.Topic);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _mqttClient?.Dispose();
    }
}

public class MqttSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string Topic { get; set; } = "iot/location";
    public string ClientIdPrefix { get; set; } = "SmartInventory-IoT";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int QosLevel { get; set; } = 1;
    public bool CleanStart { get; set; } = true;
    public int KeepAliveSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 10;
}
