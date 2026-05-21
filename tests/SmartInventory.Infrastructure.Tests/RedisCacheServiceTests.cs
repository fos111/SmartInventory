using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using SmartInventory.Infrastructure.Caching;
using Xunit;

namespace SmartInventory.Infrastructure.Tests;

public class RedisCacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _connectionMock;
    private readonly Mock<IDatabase> _dbMock;
    private readonly Mock<IServer> _serverMock;
    private readonly RedisCacheService _service;

    public RedisCacheServiceTests()
    {
        _dbMock = new Mock<IDatabase>();
        _serverMock = new Mock<IServer>();
        _connectionMock = new Mock<IConnectionMultiplexer>();

        var endPoints = new EndPoint[] { new DnsEndPoint("localhost", 6379) };
        _connectionMock.Setup(c => c.GetEndPoints(It.IsAny<bool>()))
            .Returns(endPoints);
        _connectionMock.Setup(c => c.GetServer(It.IsAny<EndPoint>(), It.IsAny<object?>()))
            .Returns(_serverMock.Object);
        _connectionMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_dbMock.Object);

        _service = new RedisCacheService(_connectionMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedValue()
    {
        var expected = new TestData { Id = 42, Name = "test" };
        var serialized = JsonSerializer.Serialize(expected, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        _dbMock.Setup(d => d.StringGetAsync(It.Is<RedisKey>(k => k == "SmartInventory:mykey"), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serialized));

        var result = await _service.GetAsync<TestData>("mykey");

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Name.Should().Be("test");
    }

    [Fact]
    public async Task GetAsync_WhenKeyMissing_ReturnsDefault()
    {
        _dbMock.Setup(d => d.StringGetAsync("SmartInventory:missing", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _service.GetAsync<TestData>("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_StoresPrefixedKeyWithSerializedValue()
    {
        var data = new TestData { Id = 99, Name = "stored" };
        RedisValue? capturedValue = null;
        TimeSpan? capturedExpiry = null;

        _dbMock.Setup(d => d.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>(
                (key, value, expiry, _, _, _) =>
                {
                    capturedValue = value;
                    capturedExpiry = expiry;
                })
            .ReturnsAsync(true);

        await _service.SetAsync("mykey", data);

        capturedValue.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<TestData>(capturedValue!.Value,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deserialized!.Id.Should().Be(99);
        deserialized.Name.Should().Be("stored");
        capturedExpiry.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithExpiration_StoresWithTtl()
    {
        _dbMock.Setup(d => d.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var ttl = TimeSpan.FromMinutes(5);
        await _service.SetAsync("ephemeral", new TestData(), ttl);

        _dbMock.Verify(d => d.StringSetAsync(
            "SmartInventory:ephemeral",
            It.IsAny<RedisValue>(),
            ttl,
            false,
            When.Always,
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_DeletesPrefixedKey()
    {
        _dbMock.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _service.RemoveAsync("toremove");

        _dbMock.Verify(d => d.KeyDeleteAsync("SmartInventory:toremove", It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_FindsKeysByPatternAndDeletesThem()
    {
        var matchingKeys = new RedisKey[] { "SmartInventory:loc:1", "SmartInventory:loc:2" };
        _serverMock.Setup(s => s.Keys(
                It.IsAny<int>(),
                It.IsAny<RedisValue>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<CommandFlags>()))
            .Returns(matchingKeys);

        await _service.RemoveByPrefixAsync("loc:");

        _dbMock.Verify(d => d.KeyDeleteAsync(matchingKeys, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WhenNoKeysMatch_DoesNothing()
    {
        _serverMock.Setup(s => s.Keys(
                It.IsAny<int>(),
                It.IsAny<RedisValue>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<CommandFlags>()))
            .Returns(Array.Empty<RedisKey>());

        await _service.RemoveByPrefixAsync("nomatch:");

        _dbMock.Verify(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
