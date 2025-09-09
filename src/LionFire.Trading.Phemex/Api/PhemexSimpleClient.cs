using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LionFire.Trading.Phemex.Configuration;

namespace LionFire.Trading.Phemex.Api;

/// <summary>
/// Simple Phemex REST API client without external dependencies
/// This provides a working implementation while we sort out CCXT integration
/// </summary>
public class PhemexSimpleClient
{
    private readonly HttpClient _httpClient;
    private readonly PhemexOptions _options;
    private readonly ILogger<PhemexSimpleClient> _logger;
    
    public PhemexSimpleClient(
        IHttpClientFactory httpClientFactory,
        IOptions<PhemexOptions> options,
        ILogger<PhemexSimpleClient> logger)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Phemex");
        
        _options.ConfigureEndpoints();
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LionFire.Trading.Phemex/1.0");
    }
    
    /// <summary>
    /// Fetch klines/candlestick data
    /// </summary>
    public async Task<List<PhemexKline>> GetKlinesAsync(
        string symbol,
        int resolution, // 60=1min, 300=5min, 900=15min, 3600=1h
        long from,
        long to)
    {
        var url = $"/exchange/public/md/kline?symbol={symbol}&resolution={resolution}&from={from}&to={to}";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PhemexKlineResponse>(json);
        
        if (result?.data?.rows == null)
            return new List<PhemexKline>();
        
        return result.data.rows.Select(row => new PhemexKline
        {
            Timestamp = (long)row[0],
            Interval = (int)row[1],
            LastClosePrice = row[2] / 10000m,
            Open = row[3] / 10000m,
            High = row[4] / 10000m,
            Low = row[5] / 10000m,
            Close = row[6] / 10000m,
            Volume = row[7],
            Turnover = row[8]
        }).ToList();
    }
    
    /// <summary>
    /// Get 24hr ticker statistics
    /// </summary>
    public async Task<PhemexTicker24hr> GetTicker24hrAsync(string symbol)
    {
        var url = $"/md/v1/ticker/24hr?symbol={symbol}";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PhemexTicker24hrResponse>(json);
        
        if (result?.result == null)
            throw new Exception("Failed to get ticker data");
        
        return new PhemexTicker24hr
        {
            Symbol = result.result.symbol,
            Open = result.result.openEp / 10000m,    // Ep suffix for price fields
            High = result.result.highEp / 10000m,
            Low = result.result.lowEp / 10000m,
            Close = result.result.lastEp / 10000m,   // Use lastEp for close
            Volume = result.result.volume,
            Turnover = result.result.turnoverEv,     // Ev suffix for turnover
            LastPrice = result.result.lastEp / 10000m,
            BidPrice = result.result.bidEp / 10000m,
            AskPrice = result.result.askEp / 10000m
        };
    }
    
    /// <summary>
    /// Get order book
    /// </summary>
    public async Task<PhemexSimpleOrderBook> GetOrderBookAsync(string symbol)
    {
        var url = $"/exchange/public/md/orderbook?symbol={symbol}";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PhemexOrderBookResponse>(json);
        
        if (result?.result?.book == null)
            throw new Exception("Failed to get order book");
        
        return new PhemexSimpleOrderBook
        {
            Symbol = symbol,
            Bids = result.result.book.bids?.Select(b => new PhemexPriceLevel
            {
                Price = b[0] / 10000m,
                Size = b[1]
            }).ToList() ?? new List<PhemexPriceLevel>(),
            Asks = result.result.book.asks?.Select(a => new PhemexPriceLevel
            {
                Price = a[0] / 10000m,
                Size = a[1]
            }).ToList() ?? new List<PhemexPriceLevel>()
        };
    }
}

#region Response Models

public class PhemexKlineResponse
{
    public int code { get; set; }
    public string msg { get; set; }
    public PhemexKlineData data { get; set; }
}

public class PhemexKlineData
{
    public long total { get; set; }
    public List<decimal[]> rows { get; set; }
}

public class PhemexTicker24hrResponse
{
    public int code { get; set; }
    public string msg { get; set; }
    public PhemexTicker24hrData result { get; set; }
}

public class PhemexTicker24hrData
{
    public string symbol { get; set; }
    public decimal openEp { get; set; }
    public decimal highEp { get; set; }
    public decimal lowEp { get; set; }
    public decimal lastEp { get; set; }
    public decimal volume { get; set; }
    public decimal turnoverEv { get; set; }
    public decimal bidEp { get; set; }
    public decimal askEp { get; set; }
    public decimal markEp { get; set; }
    public decimal indexEp { get; set; }
}

public class PhemexOrderBookResponse
{
    public int code { get; set; }
    public string msg { get; set; }
    public PhemexOrderBookResult result { get; set; }
}

public class PhemexOrderBookResult
{
    public PhemexOrderBookData book { get; set; }
}

public class PhemexOrderBookData
{
    public List<decimal[]> asks { get; set; }
    public List<decimal[]> bids { get; set; }
}

#endregion

#region Data Models

public class PhemexKline
{
    public long Timestamp { get; set; }
    public int Interval { get; set; }
    public decimal LastClosePrice { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal Turnover { get; set; }
    
    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;
}

public class PhemexTicker24hr
{
    public string Symbol { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal Turnover { get; set; }
    public decimal LastPrice { get; set; }
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
}

public class PhemexSimpleOrderBook
{
    public string Symbol { get; set; }
    public List<PhemexPriceLevel> Bids { get; set; } = new();
    public List<PhemexPriceLevel> Asks { get; set; } = new();
}

public class PhemexPriceLevel
{
    public decimal Price { get; set; }
    public decimal Size { get; set; }
}

#endregion