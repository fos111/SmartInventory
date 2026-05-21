using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace SmartInventory.Infrastructure.Caching;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connection;

    public RedisHealthCheck(IConnectionMultiplexer connection)
    {
        _connection = connection;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_connection.IsConnected)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Redis is connected"));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Redis is not connected"));
    }
}
