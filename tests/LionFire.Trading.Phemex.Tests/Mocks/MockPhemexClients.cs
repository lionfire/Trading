using System.Reactive.Linq;
using System.Reactive.Subjects;
using LionFire.Trading.Phemex.Rest;
using LionFire.Trading.Phemex.WebSocket;
using Newtonsoft.Json;

namespace LionFire.Trading.Phemex.Tests.Mocks;

public class MockPhemexWebSocketClient : IPhemexWebSocketClient
{
    private readonly Subject<string> _messageSubject = new();
    private readonly HashSet<string> _subscriptions = new();
    private Timer? _tickTimer;
    private bool _isConnected;

    public IObservable<string> Messages => _messageSubject.AsObservable();
    public bool IsConnected => _isConnected;
    public bool AutoGenerateTicks { get; set; } = true;
    public decimal BasePrice { get; set; } = 45000;
    public decimal PriceVolatility { get; set; } = 100;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = true;
        
        if (AutoGenerateTicks)
        {
            StartTickGeneration();
        }
        
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        _isConnected = false;
        StopTickGeneration();
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string channel, string symbol)
    {
        var subscription = $"{channel}.{symbol}";
        _subscriptions.Add(subscription);
        
        // Send subscription acknowledgment
        var ackMessage = JsonConvert.SerializeObject(new
        {
            @event = "subscribe",
            channel = subscription,
            success = true
        });
        _messageSubject.OnNext(ackMessage);
        
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string channel, string symbol)
    {
        var subscription = $"{channel}.{symbol}";
        _subscriptions.Remove(subscription);
        
        // Send unsubscription acknowledgment
        var ackMessage = JsonConvert.SerializeObject(new
        {
            @event = "unsubscribe",
            channel = subscription,
            success = true
        });
        _messageSubject.OnNext(ackMessage);
        
        return Task.CompletedTask;
    }

    public void SendMockTick(string symbol, decimal price, decimal volume, string side = "Buy")
    {
        var tick = new
        {
            type = "incremental",
            topic = $"trade.{symbol}",
            data = new[]
            {
                new
                {
                    symbol = symbol,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    side = side,
                    size = volume,
                    price = (long)(price * 10000), // Phemex uses scaled prices
                    tickDirection = side == "Buy" ? "PlusTick" : "MinusTick"
                }
            }
        };
        
        _messageSubject.OnNext(JsonConvert.SerializeObject(tick));
    }

    public void SendMockOrderBook(string symbol, decimal[][] bids, decimal[][] asks)
    {
        var orderBook = new
        {
            type = "snapshot",
            topic = $"book.{symbol}",
            data = new
            {
                symbol = symbol,
                bids = bids.Select(b => new[] { (long)(b[0] * 10000), (long)b[1] }).ToArray(),
                asks = asks.Select(a => new[] { (long)(a[0] * 10000), (long)a[1] }).ToArray(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };
        
        _messageSubject.OnNext(JsonConvert.SerializeObject(orderBook));
    }

    private void StartTickGeneration()
    {
        _tickTimer = new Timer(_ => GenerateRandomTick(), null, 1000, 500);
    }

    private void StopTickGeneration()
    {
        _tickTimer?.Dispose();
        _tickTimer = null;
    }

    private void GenerateRandomTick()
    {
        if (!_isConnected || !_subscriptions.Any(s => s.StartsWith("trade.")))
            return;
        
        var random = Random.Shared;
        var priceChange = (decimal)(random.NextDouble() - 0.5) * PriceVolatility;
        var price = BasePrice + priceChange;
        var volume = random.Next(1, 100);
        var side = random.Next(2) == 0 ? "Buy" : "Sell";
        
        foreach (var subscription in _subscriptions.Where(s => s.StartsWith("trade.")))
        {
            var symbol = subscription.Split('.')[1];
            SendMockTick(symbol, price, volume, side);
        }
    }
}

public class MockPhemexRestClient : IPhemexRestClient
{
    private readonly Dictionary<string, object> _mockResponses = new();
    private readonly List<(string Method, string Path, object? Body)> _requestHistory = new();
    
    public bool ThrowOnUnmockedRequest { get; set; } = false;
    public TimeSpan SimulatedLatency { get; set; } = TimeSpan.Zero;

    public IReadOnlyList<(string Method, string Path, object? Body)> RequestHistory => _requestHistory;

    public void SetupResponse<T>(string method, string path, T response)
    {
        var key = $"{method}:{path}";
        _mockResponses[key] = response!;
    }

    public void SetupGetResponse<T>(string path, T response) => SetupResponse("GET", path, response);
    public void SetupPostResponse<T>(string path, T response) => SetupResponse("POST", path, response);

    public async Task<T?> GetAsync<T>(string path, Dictionary<string, string>? parameters = null)
    {
        return await ExecuteAsync<T>("GET", path, null, parameters);
    }

    public async Task<T?> PostAsync<T>(string path, object? body = null)
    {
        return await ExecuteAsync<T>("POST", path, body, null);
    }

    public async Task<T?> PutAsync<T>(string path, object? body = null)
    {
        return await ExecuteAsync<T>("PUT", path, body, null);
    }

    public async Task<T?> DeleteAsync<T>(string path, Dictionary<string, string>? parameters = null)
    {
        return await ExecuteAsync<T>("DELETE", path, null, parameters);
    }

    private async Task<T?> ExecuteAsync<T>(string method, string path, object? body, Dictionary<string, string>? parameters)
    {
        _requestHistory.Add((method, path, body));
        
        if (SimulatedLatency > TimeSpan.Zero)
        {
            await Task.Delay(SimulatedLatency);
        }
        
        var key = $"{method}:{path}";
        
        // Check for exact match first
        if (_mockResponses.TryGetValue(key, out var response))
        {
            if (response is T typedResponse)
            {
                return typedResponse;
            }
            
            // Try to convert
            var json = JsonConvert.SerializeObject(response);
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        // Check for wildcard matches
        var wildcardKey = _mockResponses.Keys.FirstOrDefault(k => 
            k.StartsWith($"{method}:") && 
            path.StartsWith(k.Substring(method.Length + 1).Replace("*", "")));
        
        if (wildcardKey != null)
        {
            var wildcardResponse = _mockResponses[wildcardKey];
            if (wildcardResponse is T typedWildcard)
            {
                return typedWildcard;
            }
            
            var json = JsonConvert.SerializeObject(wildcardResponse);
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        if (ThrowOnUnmockedRequest)
        {
            throw new InvalidOperationException($"No mock response setup for {method} {path}");
        }
        
        return default;
    }

    public void ClearHistory()
    {
        _requestHistory.Clear();
    }

    public void ClearResponses()
    {
        _mockResponses.Clear();
    }

    public bool WasRequestMade(string method, string path)
    {
        return _requestHistory.Any(r => r.Method == method && r.Path == path);
    }

    public int GetRequestCount(string method, string path)
    {
        return _requestHistory.Count(r => r.Method == method && r.Path == path);
    }
}