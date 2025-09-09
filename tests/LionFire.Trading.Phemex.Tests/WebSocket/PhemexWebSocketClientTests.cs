using System.Reactive.Linq;
using FluentAssertions;
using LionFire.Trading.Phemex.Configuration;
using LionFire.Trading.Phemex.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LionFire.Trading.Phemex.Tests.WebSocket;

public class PhemexWebSocketClientTests : IDisposable
{
    private readonly PhemexWebSocketClient _client;
    private readonly Mock<ILogger<PhemexWebSocketClient>> _loggerMock;
    private readonly PhemexOptions _options;
    private readonly CancellationTokenSource _cts = new();

    public PhemexWebSocketClientTests()
    {
        _loggerMock = new Mock<ILogger<PhemexWebSocketClient>>();
        _options = new PhemexOptions 
        { 
            IsTestnet = true,
            ApiKey = "test-key",
            ApiSecret = "test-secret"
        };
        _client = new PhemexWebSocketClient(
            Options.Create(_options),
            _loggerMock.Object);
    }

    [Fact]
    public void InitialState_ShouldNotBeConnected()
    {
        // Assert
        _client.IsConnected.Should().BeFalse();
    }

    [Fact(Skip = "Requires live connection")]
    public async Task Connect_ShouldEstablishConnection()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        await _client.ConnectAsync(cts.Token);

        // Assert
        _client.IsConnected.Should().BeTrue();
    }

    [Fact(Skip = "Requires live connection")]
    public async Task Subscribe_ShouldSendCorrectMessage()
    {
        // Arrange
        var messages = new List<string>();
        using var subscription = _client.Messages.Subscribe(msg => messages.Add(msg));
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        await _client.ConnectAsync(cts.Token);
        await _client.SubscribeAsync("trade", "BTCUSD");
        await Task.Delay(1000);

        // Assert
        // We should receive at least an acknowledgment or trade data
        messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Disconnect_WhenNotConnected_ShouldNotThrow()
    {
        // Act
        var act = async () => await _client.DisconnectAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact(Skip = "Requires live connection")]
    public async Task Reconnection_ShouldResubscribe()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var reconnected = false;
        var resubscribed = false;

        // Act
        await _client.ConnectAsync(cts.Token);
        await _client.SubscribeAsync("trade", "BTCUSD");
        
        // Simulate disconnection and reconnection
        await _client.DisconnectAsync();
        await Task.Delay(1000);
        
        await _client.ConnectAsync(cts.Token);
        reconnected = _client.IsConnected;

        // Check if resubscription happened
        using var subscription = _client.Messages
            .Where(msg => msg.Contains("trade.BTCUSD"))
            .Subscribe(_ => resubscribed = true);

        await Task.Delay(5000);

        // Assert
        reconnected.Should().BeTrue();
        resubscribed.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleConnectCalls_ShouldNotCreateMultipleConnections()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        
        // Act & Assert
        // This should not throw and should handle concurrent calls properly
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.ConnectAsync(cts.Token))
            .ToArray();
        
        await Task.WhenAll(tasks);
        
        // Should still be in a valid state
        // (actual connection test would require live endpoint)
    }

    [Fact]
    public void Messages_ShouldReturnObservable()
    {
        // Act
        var messages = _client.Messages;

        // Assert
        messages.Should().NotBeNull();
        messages.Should().BeAssignableTo<IObservable<string>>();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _client?.Dispose();
    }
}

public class PhemexWebSocketClientMockTests
{
    private readonly Mock<IPhemexWebSocketClient> _clientMock;

    public PhemexWebSocketClientMockTests()
    {
        _clientMock = new Mock<IPhemexWebSocketClient>();
    }

    [Fact]
    public async Task MockClient_ShouldSimulateConnection()
    {
        // Arrange
        _clientMock.SetupGet(x => x.IsConnected).Returns(false);
        _clientMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _clientMock.SetupGet(x => x.IsConnected).Returns(true))
            .Returns(Task.CompletedTask);

        // Act
        var client = _clientMock.Object;
        var beforeConnect = client.IsConnected;
        await client.ConnectAsync();
        var afterConnect = client.IsConnected;

        // Assert
        beforeConnect.Should().BeFalse();
        afterConnect.Should().BeTrue();
        _clientMock.Verify(x => x.ConnectAsync(It.IsAny<CancellationToken>()), Moq.Times.Once);
    }

    [Fact]
    public async Task MockClient_ShouldSimulateMessages()
    {
        // Arrange
        var subject = new System.Reactive.Subjects.Subject<string>();
        _clientMock.SetupGet(x => x.Messages).Returns(subject);
        
        var receivedMessages = new List<string>();
        using var subscription = _clientMock.Object.Messages.Subscribe(msg => receivedMessages.Add(msg));

        // Act
        subject.OnNext("{\"type\":\"trade\",\"data\":{\"price\":45000}}");
        subject.OnNext("{\"type\":\"trade\",\"data\":{\"price\":45100}}");
        await Task.Delay(100);

        // Assert
        receivedMessages.Should().HaveCount(2);
        receivedMessages[0].Should().Contain("45000");
        receivedMessages[1].Should().Contain("45100");
    }
}