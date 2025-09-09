using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using LionFire.Trading.Phemex.Configuration;

namespace LionFire.Trading.Phemex.WebSocket;

public interface IPhemexWebSocketClient
{
    IObservable<string> Messages { get; }
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task SubscribeAsync(string channel, string symbol);
    Task UnsubscribeAsync(string channel, string symbol);
}

public class PhemexWebSocketClient : IPhemexWebSocketClient, IDisposable
{
    private readonly ILogger<PhemexWebSocketClient> _logger;
    private readonly PhemexOptions _options;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Subject<string> _messageSubject = new();
    private readonly HashSet<string> _subscriptions = new();
    private Task? _receiveTask;
    private readonly SemaphoreSlim _connectSemaphore = new(1, 1);

    public IObservable<string> Messages => _messageSubject.AsObservable();
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public PhemexWebSocketClient(
        IOptions<PhemexOptions> options,
        ILogger<PhemexWebSocketClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                _logger.LogDebug("WebSocket already connected");
                return;
            }

            await DisconnectInternalAsync();

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            _logger.LogInformation("Connecting to Phemex WebSocket at {Url}", _options.WebSocketUrl);
            await _webSocket.ConnectAsync(new Uri(_options.WebSocketUrl), cancellationToken);
            
            _logger.LogInformation("WebSocket connected successfully");

            // Start receive loop
            _receiveTask = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));

            // Resubscribe to previous subscriptions
            foreach (var subscription in _subscriptions.ToList())
            {
                var parts = subscription.Split('.');
                if (parts.Length == 2)
                {
                    await SendSubscribeMessage(parts[0], parts[1]);
                }
            }
        }
        finally
        {
            _connectSemaphore.Release();
        }
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        var messageBuilder = new List<byte>();

        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        messageBuilder.AddRange(buffer.Take(result.Count));

                        if (result.EndOfMessage)
                        {
                            var message = Encoding.UTF8.GetString(messageBuilder.ToArray());
                            messageBuilder.Clear();

                            _logger.LogTrace("Received message: {Message}", message);
                            _messageSubject.OnNext(message);

                            // Handle ping/pong
                            if (message.Contains("\"method\":\"ping\""))
                            {
                                await SendPongAsync();
                            }
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("WebSocket closed by server");
                        break;
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogError(ex, "WebSocket error in receive loop");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Receive loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in receive loop");
        }

        // Trigger reconnection if not intentionally disconnected
        if (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("WebSocket disconnected unexpectedly, attempting reconnection...");
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                await ConnectAsync();
            });
        }
    }

    private async Task SendPongAsync()
    {
        var pong = JsonConvert.SerializeObject(new { method = "pong" });
        await SendMessageAsync(pong);
    }

    public async Task SubscribeAsync(string channel, string symbol)
    {
        var subscription = $"{channel}.{symbol}";
        _subscriptions.Add(subscription);

        if (IsConnected)
        {
            await SendSubscribeMessage(channel, symbol);
        }
    }

    private async Task SendSubscribeMessage(string channel, string symbol)
    {
        var subscribeMessage = JsonConvert.SerializeObject(new
        {
            method = "subscribe",
            @params = new[] { $"{channel}.{symbol}" },
            id = Guid.NewGuid().ToString()
        });

        await SendMessageAsync(subscribeMessage);
        _logger.LogInformation("Subscribed to {Channel}.{Symbol}", channel, symbol);
    }

    public async Task UnsubscribeAsync(string channel, string symbol)
    {
        var subscription = $"{channel}.{symbol}";
        _subscriptions.Remove(subscription);

        if (IsConnected)
        {
            var unsubscribeMessage = JsonConvert.SerializeObject(new
            {
                method = "unsubscribe",
                @params = new[] { subscription },
                id = Guid.NewGuid().ToString()
            });

            await SendMessageAsync(unsubscribeMessage);
            _logger.LogInformation("Unsubscribed from {Channel}.{Symbol}", channel, symbol);
        }
    }

    private async Task SendMessageAsync(string message)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        _logger.LogTrace("Sent message: {Message}", message);
    }

    public async Task DisconnectAsync()
    {
        await _connectSemaphore.WaitAsync();
        try
        {
            await DisconnectInternalAsync();
        }
        finally
        {
            _connectSemaphore.Release();
        }
    }

    private async Task DisconnectInternalAsync()
    {
        try
        {
            _cancellationTokenSource?.Cancel();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }

            if (_receiveTask != null)
            {
                await _receiveTask;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
        }
        finally
        {
            _webSocket?.Dispose();
            _webSocket = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        _messageSubject.Dispose();
        _connectSemaphore.Dispose();
    }
}