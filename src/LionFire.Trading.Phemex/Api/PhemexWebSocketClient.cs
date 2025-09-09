using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LionFire.Trading.Phemex.Configuration;

namespace LionFire.Trading.Phemex.Api;

public class PhemexWebSocketClient : IDisposable
{
    private readonly PhemexOptions options;
    private readonly ILogger<PhemexWebSocketClient> logger;
    private ClientWebSocket? webSocket;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? receiveTask;
    private readonly SemaphoreSlim connectSemaphore = new(1, 1);
    
    // Subjects for different data streams
    private readonly Subject<PhemexTick> tickSubject = new();
    private readonly Subject<PhemexOrderBook> orderBookSubject = new();
    private readonly Subject<PhemexTrade> tradeSubject = new();
    private readonly Subject<string> connectionStatusSubject = new();
    
    public IObservable<PhemexTick> TickStream => tickSubject.AsObservable();
    public IObservable<PhemexOrderBook> OrderBookStream => orderBookSubject.AsObservable();
    public IObservable<PhemexTrade> TradeStream => tradeSubject.AsObservable();
    public IObservable<string> ConnectionStatus => connectionStatusSubject.AsObservable();
    
    public bool IsConnected => webSocket?.State == WebSocketState.Open;
    
    public PhemexWebSocketClient(
        IOptions<PhemexOptions> options,
        ILogger<PhemexWebSocketClient> logger)
    {
        this.options = options.Value;
        this.logger = logger;
        
        // Configure endpoints based on settings
        this.options.ConfigureEndpoints();
        
        logger.LogInformation("Phemex WebSocket client configured - Endpoint: {WebSocketUrl}", 
            this.options.WebSocketUrl);
    }
    
    public async Task ConnectAsync()
    {
        await connectSemaphore.WaitAsync();
        try
        {
            if (IsConnected)
                return;
                
            cancellationTokenSource = new CancellationTokenSource();
            webSocket = new ClientWebSocket();
            
            logger.LogInformation("Connecting to Phemex WebSocket: {Url}", options.WebSocketUrl);
            await webSocket.ConnectAsync(new Uri(options.WebSocketUrl), cancellationTokenSource.Token);
            
            connectionStatusSubject.OnNext("Connected");
            logger.LogInformation("Connected to Phemex WebSocket");
            
            // Start receive loop
            receiveTask = Task.Run(ReceiveLoop, cancellationTokenSource.Token);
            
            // Start ping task to keep connection alive
            _ = Task.Run(PingLoop, cancellationTokenSource.Token);
            
            // Authenticate if API keys are provided
            await AuthenticateAsync();
        }
        finally
        {
            connectSemaphore.Release();
        }
    }
    
    public async Task DisconnectAsync()
    {
        await connectSemaphore.WaitAsync();
        try
        {
            if (webSocket?.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            
            cancellationTokenSource?.Cancel();
            webSocket?.Dispose();
            webSocket = null;
            
            connectionStatusSubject.OnNext("Disconnected");
            logger.LogInformation("Disconnected from Phemex WebSocket");
        }
        finally
        {
            connectSemaphore.Release();
        }
    }
    
    public async Task SubscribeToTickerAsync(params string[] symbols)
    {
        var subscription = new
        {
            id = Guid.NewGuid().ToString(),
            method = "tick.subscribe",
            @params = symbols
        };
        
        await SendMessageAsync(JsonConvert.SerializeObject(subscription));
        logger.LogInformation("Subscribed to ticker for symbols: {Symbols}", string.Join(", ", symbols));
    }
    
    public async Task SubscribeToOrderBookAsync(params string[] symbols)
    {
        var subscription = new
        {
            id = Guid.NewGuid().ToString(),
            method = "orderbook.subscribe",
            @params = symbols
        };
        
        await SendMessageAsync(JsonConvert.SerializeObject(subscription));
        logger.LogInformation("Subscribed to order book for symbols: {Symbols}", string.Join(", ", symbols));
    }
    
    public async Task SubscribeToTradesAsync(params string[] symbols)
    {
        var subscription = new
        {
            id = Guid.NewGuid().ToString(),
            method = "trade.subscribe",
            @params = symbols
        };
        
        await SendMessageAsync(JsonConvert.SerializeObject(subscription));
        logger.LogInformation("Subscribed to trades for symbols: {Symbols}", string.Join(", ", symbols));
    }
    
    private async Task AuthenticateAsync()
    {
        if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
            return;
            
        var expiry = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
        var signature = CreateSignature($"GET/realtime{expiry}");
        
        var authMessage = new
        {
            method = "user.auth",
            @params = new object[]
            {
                "API",
                options.ApiKey,
                signature,
                expiry
            }
        };
        
        await SendMessageAsync(JsonConvert.SerializeObject(authMessage));
        logger.LogInformation("Sent authentication message");
    }
    
    private string CreateSignature(string data)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            Encoding.UTF8.GetBytes(options.ApiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
    
    private async Task SendMessageAsync(string message)
    {
        if (!IsConnected)
            throw new InvalidOperationException("WebSocket is not connected");
            
        var bytes = Encoding.UTF8.GetBytes(message);
        await webSocket!.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
    
    private async Task ReceiveLoop()
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        var messageBuilder = new StringBuilder();
        
        while (!cancellationTokenSource!.Token.IsCancellationRequested && IsConnected)
        {
            try
            {
                var result = await webSocket!.ReceiveAsync(buffer, cancellationTokenSource.Token);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer.Array!, 0, result.Count));
                    
                    if (result.EndOfMessage)
                    {
                        ProcessMessage(messageBuilder.ToString());
                        messageBuilder.Clear();
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await DisconnectAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in WebSocket receive loop");
                connectionStatusSubject.OnNext("Error");
                
                // Attempt reconnection after delay
                await Task.Delay(options.WebSocketReconnectDelay);
                await ReconnectAsync();
            }
        }
    }
    
    private void ProcessMessage(string message)
    {
        try
        {
            var json = JObject.Parse(message);
            
            // Check for different message types
            if (json["type"]?.ToString() == "tick")
            {
                var tick = json["data"]?.ToObject<PhemexTick>();
                if (tick != null)
                    tickSubject.OnNext(tick);
            }
            else if (json["type"]?.ToString() == "orderbook")
            {
                var orderBook = json["data"]?.ToObject<PhemexOrderBook>();
                if (orderBook != null)
                    orderBookSubject.OnNext(orderBook);
            }
            else if (json["type"]?.ToString() == "trade")
            {
                var trade = json["data"]?.ToObject<PhemexTrade>();
                if (trade != null)
                    tradeSubject.OnNext(trade);
            }
            else
            {
                logger.LogDebug("Received message: {Message}", message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
        }
    }
    
    private async Task PingLoop()
    {
        while (!cancellationTokenSource!.Token.IsCancellationRequested && IsConnected)
        {
            try
            {
                await Task.Delay(options.WebSocketPingInterval, cancellationTokenSource.Token);
                
                var pingMessage = new { method = "server.ping" };
                await SendMessageAsync(JsonConvert.SerializeObject(pingMessage));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending ping");
            }
        }
    }
    
    private async Task ReconnectAsync()
    {
        logger.LogInformation("Attempting to reconnect to WebSocket");
        await DisconnectAsync();
        await ConnectAsync();
        connectionStatusSubject.OnNext("Reconnected");
    }
    
    public void Dispose()
    {
        DisconnectAsync().Wait();
        tickSubject.Dispose();
        orderBookSubject.Dispose();
        tradeSubject.Dispose();
        connectionStatusSubject.Dispose();
        connectSemaphore.Dispose();
    }
}

public class PhemexTick
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("bidEp")]
    public long BidPriceEp { get; set; }
    
    [JsonProperty("askEp")]
    public long AskPriceEp { get; set; }
    
    [JsonProperty("lastEp")]
    public long LastPriceEp { get; set; }
    
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
    
    public decimal BidPrice => BidPriceEp / 10000m;
    public decimal AskPrice => AskPriceEp / 10000m;
    public decimal LastPrice => LastPriceEp / 10000m;
    public DateTime Time => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
}

public class PhemexOrderBook
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("bids")]
    public List<List<decimal>> Bids { get; set; } = new();
    
    [JsonProperty("asks")]
    public List<List<decimal>> Asks { get; set; } = new();
    
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}

public class PhemexTrade
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("priceEp")]
    public long PriceEp { get; set; }
    
    [JsonProperty("qty")]
    public decimal Quantity { get; set; }
    
    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;
    
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
    
    public decimal Price => PriceEp / 10000m;
    public DateTime Time => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
}