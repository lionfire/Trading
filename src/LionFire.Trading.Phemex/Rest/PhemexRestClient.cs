using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LionFire.Trading.Phemex.Configuration;

namespace LionFire.Trading.Phemex.Rest;

public interface IPhemexRestClient
{
    Task<T?> GetAsync<T>(string path, Dictionary<string, string>? parameters = null);
    Task<T?> PostAsync<T>(string path, object? body = null);
    Task<T?> PutAsync<T>(string path, object? body = null);
    Task<T?> DeleteAsync<T>(string path, Dictionary<string, string>? parameters = null);
}

public class PhemexRestClient : IPhemexRestClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PhemexRestClient> _logger;
    private readonly PhemexOptions _options;
    private readonly SemaphoreSlim _rateLimiter;

    public PhemexRestClient(
        IOptions<PhemexOptions> options,
        ILogger<PhemexRestClient> logger,
        HttpClient? httpClient = null)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(_options.BaseUrl) };
        _rateLimiter = new SemaphoreSlim(_options.RateLimitPerSecond, _options.RateLimitPerSecond);
        
        SetupHttpClient();
    }

    private void SetupHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LionFire.Trading.Phemex/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<T?> GetAsync<T>(string path, Dictionary<string, string>? parameters = null)
    {
        var queryString = BuildQueryString(parameters);
        var fullPath = string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, fullPath);
        return await SendRequestAsync<T>(request, fullPath);
    }

    public async Task<T?> PostAsync<T>(string path, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        
        if (body != null)
        {
            var json = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        return await SendRequestAsync<T>(request, path, body);
    }

    public async Task<T?> PutAsync<T>(string path, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path);
        
        if (body != null)
        {
            var json = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        return await SendRequestAsync<T>(request, path, body);
    }

    public async Task<T?> DeleteAsync<T>(string path, Dictionary<string, string>? parameters = null)
    {
        var queryString = BuildQueryString(parameters);
        var fullPath = string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}";
        
        var request = new HttpRequestMessage(HttpMethod.Delete, fullPath);
        return await SendRequestAsync<T>(request, fullPath);
    }

    private async Task<T?> SendRequestAsync<T>(HttpRequestMessage request, string path, object? body = null)
    {
        await _rateLimiter.WaitAsync();
        
        try
        {
            // Add authentication headers if this is a private endpoint
            if (RequiresAuthentication(path))
            {
                AddAuthenticationHeaders(request, path, body);
            }

            _logger.LogDebug("Sending {Method} request to {Path}", request.Method, path);
            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogTrace("Response: {StatusCode} - {Content}", response.StatusCode, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Request failed: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Request failed with status {response.StatusCode}: {content}");
            }

            // Handle different response formats
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            // Try to parse as JSON
            try
            {
                var jsonResponse = JObject.Parse(content);
                
                // Check for error in response
                if (jsonResponse["error"] != null && jsonResponse["error"].Type != JTokenType.Null)
                {
                    var error = jsonResponse["error"].ToString();
                    throw new Exception($"API Error: {error}");
                }
                
                // Extract result if present
                if (jsonResponse["result"] != null)
                {
                    return jsonResponse["result"].ToObject<T>();
                }
                
                // Otherwise return the whole response
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (JsonException)
            {
                // If not JSON, try to convert directly
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)content;
                }
                throw;
            }
        }
        finally
        {
            // Release rate limiter after 1 second
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                _rateLimiter.Release();
            });
        }
    }

    private bool RequiresAuthentication(string path)
    {
        // Public endpoints that don't require authentication
        var publicEndpoints = new[]
        {
            "/md/",
            "/public/",
            "/v1/md/",
            "/v2/md/",
            "/cfg/v2/products"
        };
        
        return !publicEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    private void AddAuthenticationHeaders(HttpRequestMessage request, string path, object? body)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var expires = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 60000).ToString();
        
        // Build the message to sign
        var message = new StringBuilder();
        message.Append(request.Method.ToString().ToUpper());
        message.Append(path);
        message.Append(expires);
        
        if (body != null && request.Method != HttpMethod.Get)
        {
            message.Append(JsonConvert.SerializeObject(body));
        }
        
        var signature = CalculateHmacSha256(message.ToString(), _options.ApiSecret);
        
        request.Headers.Add("x-phemex-access-token", _options.ApiKey);
        request.Headers.Add("x-phemex-request-expiry", expires);
        request.Headers.Add("x-phemex-request-signature", signature);
        
        if (_options.SubAccountId.HasValue)
        {
            request.Headers.Add("x-phemex-subaccount-id", _options.SubAccountId.Value.ToString());
        }
    }

    public static string CalculateHmacSha256(string message, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private string BuildQueryString(Dictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return string.Empty;
        }
        
        return string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _rateLimiter?.Dispose();
    }
}