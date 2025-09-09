using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using LionFire.Trading.Phemex.Api.Models;
using LionFire.Trading.Phemex.Configuration;

namespace LionFire.Trading.Phemex.Api;

public class PhemexRestClient
{
    private readonly HttpClient httpClient;
    private readonly PhemexOptions options;
    private readonly ILogger<PhemexRestClient> logger;
    private readonly SemaphoreSlim rateLimiter;
    private DateTime lastRequestTime = DateTime.MinValue;
    
    public PhemexRestClient(
        HttpClient httpClient,
        IOptions<PhemexOptions> options,
        ILogger<PhemexRestClient> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
        this.rateLimiter = new SemaphoreSlim(1, 1);
        
        // Configure endpoints based on settings
        this.options.ConfigureEndpoints();
        this.options.EnsureValid();
        ConfigureHttpClient();
        
        logger.LogInformation("Phemex REST client configured - Endpoint: {BaseUrl}, Rate limit: {RateLimit}/sec", 
            this.options.BaseUrl, this.options.RateLimitPerSecond);
    }
    
    private void ConfigureHttpClient()
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);
        httpClient.Timeout = options.ConnectionTimeout;
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    #region Account Methods
    
    public async Task<PhemexAccountPosition?> GetAccountPositionsAsync(string currency = "USD", long? accountId = null)
    {
        var path = $"/accounts/accountPositions?currency={currency}";
        if (accountId.HasValue)
        {
            path += $"&accountID={accountId}";
        }
        var response = await SendSignedRequestAsync<PhemexAccountPosition>(HttpMethod.Get, path);
        return response;
    }
    
    public async Task<PhemexAccountInfo?> GetAccountInfoAsync(string currency = "USD", long? accountId = null)
    {
        var path = $"/accounts/positions?currency={currency}";
        if (accountId.HasValue)
        {
            path += $"&accountID={accountId}";
        }
        var response = await SendSignedRequestAsync<PhemexAccountInfo>(HttpMethod.Get, path);
        return response;
    }
    
    public async Task<List<PhemexSubAccount>> GetSubAccountsAsync()
    {
        var path = "/phemex-user/users/children";
        var response = await SendSignedRequestAsync<PhemexSubAccountsResponse>(HttpMethod.Get, path);
        return response?.Data?.Rows ?? new List<PhemexSubAccount>();
    }
    
    public async Task<bool> SwitchToSubAccountAsync(long subAccountId)
    {
        // This would need to be implemented based on Phemex's subaccount switching mechanism
        // Typically involves setting specific headers or using different API credentials
        logger.LogInformation("Switching to subaccount: {SubAccountId}", subAccountId);
        
        // For now, this is a placeholder
        // Actual implementation would depend on Phemex API documentation
        return true;
    }
    
    #endregion
    
    #region Order Methods
    
    public async Task<PhemexOrder?> PlaceOrderAsync(PhemexOrderRequest request)
    {
        var path = "/orders";
        var response = await SendSignedRequestAsync<PhemexOrderResponse>(
            HttpMethod.Post, 
            path, 
            JsonConvert.SerializeObject(request));
            
        if (response?.Code != 0)
        {
            logger.LogError("Failed to place order: {Code} - {Message}", response?.Code, response?.Message);
            return null;
        }
        
        return response?.Data;
    }
    
    public async Task<bool> CancelOrderAsync(string orderId, string symbol)
    {
        var path = $"/orders/cancel?orderID={orderId}&symbol={symbol}";
        var response = await SendSignedRequestAsync<PhemexOrderResponse>(HttpMethod.Delete, path);
        return response?.Code == 0;
    }
    
    public async Task<List<PhemexOrder>> GetOpenOrdersAsync(string? symbol = null)
    {
        var path = "/orders/activeList";
        if (!string.IsNullOrEmpty(symbol))
            path += $"?symbol={symbol}";
            
        var response = await SendSignedRequestAsync<PhemexOrderListResponse>(HttpMethod.Get, path);
        return response?.Data?.Rows ?? new List<PhemexOrder>();
    }
    
    #endregion
    
    #region Private Methods
    
    private async Task<T?> SendSignedRequestAsync<T>(HttpMethod method, string path, string? body = null)
    {
        await EnforceRateLimit();
        
        try
        {
            var request = new HttpRequestMessage(method, path);
            
            // Add authentication headers
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var expires = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60).ToString(); // 60 seconds expiry
            
            request.Headers.Add("x-phemex-access-token", options.ApiKey);
            request.Headers.Add("x-phemex-request-expiry", expires);
            
            // Create signature
            var signatureData = $"{path}{expires}{body ?? ""}";
            var signature = CreateSignature(signatureData);
            request.Headers.Add("x-phemex-request-signature", signature);
            
            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }
            
            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Phemex API error: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);
                return default;
            }
            
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Phemex API: {Path}", path);
            return default;
        }
    }
    
    private string CreateSignature(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.ApiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
    
    private async Task EnforceRateLimit()
    {
        await rateLimiter.WaitAsync();
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
            var minInterval = TimeSpan.FromMilliseconds(1000.0 / options.RateLimitPerSecond);
            
            if (timeSinceLastRequest < minInterval)
            {
                await Task.Delay(minInterval - timeSinceLastRequest);
            }
            
            lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            rateLimiter.Release();
        }
    }
    
    #endregion
    
    private class PhemexOrderListResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        
        [JsonProperty("msg")]
        public string Message { get; set; } = string.Empty;
        
        [JsonProperty("data")]
        public PhemexOrderListData? Data { get; set; }
    }
    
    private class PhemexOrderListData
    {
        [JsonProperty("rows")]
        public List<PhemexOrder> Rows { get; set; } = new();
    }
}