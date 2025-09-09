namespace LionFire.Trading.Phemex.Configuration;

public class PhemexOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool IsTestnet { get; set; } = true;
    
    /// <summary>
    /// Use high-rate-limit API endpoint (vapi.phemex.com)
    /// Requires special API access; provides higher rate limits
    /// </summary>
    public bool UseHighRateLimitApi { get; set; } = false;
    
    /// <summary>
    /// Subaccount ID to use for trading (optional)
    /// If specified, all API calls will be made on behalf of this subaccount
    /// </summary>
    public long? SubAccountId { get; set; }
    
    /// <summary>
    /// Base URL for API requests. Auto-configured based on IsTestnet and UseHighRateLimitApi
    /// </summary>
    public string BaseUrl { get; set; } = "https://testnet-api.phemex.com";
    
    /// <summary>
    /// Rate limit per second
    /// Standard API: 10 requests/second
    /// High-rate API: 100+ requests/second (depends on account tier)
    /// </summary>
    public int RateLimitPerSecond { get; set; } = 10;
    
    /// <summary>
    /// Connection timeout for REST API calls
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// WebSocket ping interval to keep connection alive
    /// </summary>
    public TimeSpan WebSocketPingInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// WebSocket reconnect delay after disconnection
    /// </summary>
    public TimeSpan WebSocketReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);
    
    public string WebSocketUrl => IsTestnet 
        ? "wss://testnet-api.phemex.com/ws" 
        : UseHighRateLimitApi 
            ? "wss://vapi.phemex.com/ws"
            : "wss://phemex.com/ws";
        
    /// <summary>
    /// Configures the BaseUrl automatically based on IsTestnet and UseHighRateLimitApi settings
    /// </summary>
    public void ConfigureEndpoints()
    {
        if (IsTestnet)
        {
            BaseUrl = "https://testnet-api.phemex.com";
            // Testnet doesn't support high-rate API
            UseHighRateLimitApi = false;
        }
        else if (UseHighRateLimitApi)
        {
            BaseUrl = "https://vapi.phemex.com";
            // High-rate API has much higher limits
            if (RateLimitPerSecond == 10) // Only update if using default
                RateLimitPerSecond = 100;
        }
        else
        {
            BaseUrl = "https://api.phemex.com";
        }
    }
        
    public void EnsureValid()
    {
        if (string.IsNullOrEmpty(ApiKey))
            throw new InvalidOperationException("Phemex ApiKey is required");
            
        if (string.IsNullOrEmpty(ApiSecret))
            throw new InvalidOperationException("Phemex ApiSecret is required");
            
        if (string.IsNullOrEmpty(BaseUrl))
        {
            // Auto-configure if not set
            ConfigureEndpoints();
        }
        
        if (string.IsNullOrEmpty(BaseUrl))
            throw new InvalidOperationException("Phemex BaseUrl is required");
    }
}