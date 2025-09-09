using System.Net;
using System.Text;
using FluentAssertions;
using LionFire.Trading.Phemex.Configuration;
using LionFire.Trading.Phemex.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace LionFire.Trading.Phemex.Tests.Rest;

public class PhemexRestClientTests : IDisposable
{
    private readonly Mock<ILogger<PhemexRestClient>> _loggerMock;
    private readonly PhemexOptions _options;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly PhemexRestClient _client;

    public PhemexRestClientTests()
    {
        _loggerMock = new Mock<ILogger<PhemexRestClient>>();
        _options = new PhemexOptions
        {
            IsTestnet = true,
            ApiKey = "test-api-key",
            ApiSecret = "test-api-secret",
            RateLimitPerSecond = 10
        };
        
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };
        
        _client = new PhemexRestClient(
            Options.Create(_options),
            _loggerMock.Object,
            _httpClient);
    }

    [Fact]
    public void SignatureCalculation_ShouldMatchExpected()
    {
        // Arrange
        var message = "GET/accounts/accountPositions60000";
        var secret = "test_secret";
        
        // Act
        var signature = PhemexRestClient.CalculateHmacSha256(message, secret);
        
        // Assert
        signature.Should().NotBeNullOrEmpty();
        signature.Should().HaveLength(64); // SHA256 produces 64 hex characters
        signature.Should().MatchRegex("^[a-f0-9]{64}$");
    }

    [Fact]
    public void SignatureCalculation_ShouldBeConsistent()
    {
        // Arrange
        var message = "GET/accounts/accountPositions60000";
        var secret = "test_secret";
        
        // Act
        var signature1 = PhemexRestClient.CalculateHmacSha256(message, secret);
        var signature2 = PhemexRestClient.CalculateHmacSha256(message, secret);
        
        // Assert
        signature1.Should().Be(signature2);
    }

    [Fact]
    public void SignatureCalculation_DifferentMessages_ShouldProduceDifferentSignatures()
    {
        // Arrange
        var secret = "test_secret";
        
        // Act
        var signature1 = PhemexRestClient.CalculateHmacSha256("message1", secret);
        var signature2 = PhemexRestClient.CalculateHmacSha256("message2", secret);
        
        // Assert
        signature1.Should().NotBe(signature2);
    }

    [Fact]
    public async Task GetAsync_PublicEndpoint_ShouldNotIncludeAuthHeaders()
    {
        // Arrange
        var responseData = new { symbol = "BTCUSD", lastPrice = 45000 };
        var responseJson = JsonConvert.SerializeObject(new { result = responseData });
        
        HttpRequestMessage? capturedRequest = null;
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        
        // Act
        var result = await _client.GetAsync<dynamic>("/md/v1/ticker/24hr", new Dictionary<string, string> { ["symbol"] = "BTCUSD" });
        
        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Contains("x-phemex-access-token").Should().BeFalse();
        capturedRequest.Headers.Contains("x-phemex-request-signature").Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_PrivateEndpoint_ShouldIncludeAuthHeaders()
    {
        // Arrange
        var responseData = new { currency = "BTC", balance = 1.5 };
        var responseJson = JsonConvert.SerializeObject(new { result = responseData });
        
        HttpRequestMessage? capturedRequest = null;
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        
        // Act
        var result = await _client.GetAsync<dynamic>("/accounts/positions");
        
        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Contains("x-phemex-access-token").Should().BeTrue();
        capturedRequest.Headers.Contains("x-phemex-request-signature").Should().BeTrue();
        capturedRequest.Headers.Contains("x-phemex-request-expiry").Should().BeTrue();
        capturedRequest.Headers.GetValues("x-phemex-access-token").First().Should().Be(_options.ApiKey);
    }

    [Fact]
    public async Task PostAsync_ShouldSendJsonBody()
    {
        // Arrange
        var requestBody = new { symbol = "BTCUSD", side = "Buy", orderQty = 1, price = 45000 };
        var responseJson = JsonConvert.SerializeObject(new { result = new { orderId = "12345" } });
        
        HttpRequestMessage? capturedRequest = null;
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        
        // Act
        var result = await _client.PostAsync<dynamic>("/orders", requestBody);
        
        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.Content.Should().NotBeNull();
        
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        content.Should().Contain("BTCUSD");
        content.Should().Contain("Buy");
    }

    [Fact]
    public async Task GetAsync_ErrorResponse_ShouldThrowException()
    {
        // Arrange
        var errorResponse = JsonConvert.SerializeObject(new { error = "Invalid API key", code = 401 });
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK, // Phemex returns 200 with error in body
                Content = new StringContent(errorResponse, Encoding.UTF8, "application/json")
            });
        
        // Act
        var act = async () => await _client.GetAsync<dynamic>("/accounts/positions");
        
        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid API key*");
    }

    [Fact]
    public async Task GetAsync_HttpError_ShouldThrowHttpRequestException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });
        
        // Act
        var act = async () => await _client.GetAsync<dynamic>("/accounts/positions");
        
        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task RateLimiting_ShouldThrottleRequests()
    {
        // Arrange
        var responseJson = JsonConvert.SerializeObject(new { result = "ok" });
        var requestCount = 0;
        var requestTimes = new List<DateTime>();
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((_, _) =>
            {
                requestCount++;
                requestTimes.Add(DateTime.UtcNow);
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        
        // Act
        var tasks = Enumerable.Range(0, 15) // Try to send 15 requests
            .Select(_ => _client.GetAsync<dynamic>("/md/v1/ticker/24hr"))
            .ToArray();
        
        await Task.WhenAll(tasks);
        
        // Assert
        requestCount.Should().Be(15);
        
        // Check that requests are throttled (not all sent immediately)
        var firstBatch = requestTimes.Take(10).Max();
        var secondBatch = requestTimes.Skip(10).Min();
        (secondBatch - firstBatch).TotalMilliseconds.Should().BeGreaterThan(900); // At least 900ms delay
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _client?.Dispose();
    }
}

public class PhemexRestClientIntegrationTests
{
    [Fact(Skip = "Requires live Phemex testnet connection")]
    public async Task GetTicker_LiveTestnet_ShouldReturnData()
    {
        // Arrange
        var options = new PhemexOptions { IsTestnet = true };
        var logger = new Mock<ILogger<PhemexRestClient>>().Object;
        using var client = new PhemexRestClient(Options.Create(options), logger);
        
        // Act
        var result = await client.GetAsync<dynamic>("/md/v1/ticker/24hr", 
            new Dictionary<string, string> { ["symbol"] = "BTCUSD" });
        
        // Assert
        result.Should().NotBeNull();
        result.symbol.Should().NotBeNull();
        result.lastPrice.Should().NotBeNull();
    }
}