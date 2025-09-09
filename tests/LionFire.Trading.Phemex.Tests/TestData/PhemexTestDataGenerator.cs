using LionFire.Trading.Abstractions;

namespace LionFire.Trading.Phemex.Tests.TestData;

public static class PhemexTestDataGenerator
{
    private static readonly Random Random = new();
    
    public static class Symbols
    {
        public const string BTCUSD = "BTCUSD";
        public const string ETHUSD = "ETHUSD";
        public const string XRPUSD = "XRPUSD";
        public const string LTCUSD = "LTCUSD";
        
        public static readonly string[] All = { BTCUSD, ETHUSD, XRPUSD, LTCUSD };
    }
    
    public static class Prices
    {
        public static readonly Dictionary<string, (decimal Min, decimal Max)> Ranges = new()
        {
            [Symbols.BTCUSD] = (40000m, 50000m),
            [Symbols.ETHUSD] = (2500m, 3500m),
            [Symbols.XRPUSD] = (0.4m, 0.8m),
            [Symbols.LTCUSD] = (80m, 120m)
        };
    }

    public static IEnumerable<PhemexTick> GenerateTicks(
        string symbol,
        int count,
        decimal? startPrice = null,
        decimal volatilityPercent = 0.5m)
    {
        var (minPrice, maxPrice) = Prices.Ranges.ContainsKey(symbol) 
            ? Prices.Ranges[symbol] 
            : (1000m, 2000m);
        
        var price = startPrice ?? (minPrice + maxPrice) / 2;
        var timestamp = DateTimeOffset.UtcNow;
        
        for (int i = 0; i < count; i++)
        {
            // Random walk with mean reversion
            var change = (decimal)(Random.NextDouble() - 0.5) * volatilityPercent / 100 * price;
            price += change;
            
            // Mean reversion
            var center = (minPrice + maxPrice) / 2;
            var reversion = (center - price) * 0.01m;
            price += reversion;
            
            // Ensure within bounds
            price = Math.Max(minPrice, Math.Min(maxPrice, price));
            
            yield return new PhemexTick
            {
                Symbol = symbol,
                Price = price,
                Volume = Random.Next(1, 1000),
                Timestamp = timestamp.AddMilliseconds(i * 100),
                Side = Random.Next(2) == 0 ? TradeSide.Buy : TradeSide.Sell,
                TradeId = Guid.NewGuid().ToString()
            };
        }
    }

    public static IEnumerable<PhemexKlineItem> GenerateKlines(
        string symbol,
        int count,
        string interval = "1m",
        DateTimeOffset? startTime = null)
    {
        var (minPrice, maxPrice) = Prices.Ranges.ContainsKey(symbol) 
            ? Prices.Ranges[symbol] 
            : (1000m, 2000m);
        
        var intervalSpan = ParseInterval(interval);
        var timestamp = startTime ?? DateTimeOffset.UtcNow.AddMinutes(-count);
        var price = (minPrice + maxPrice) / 2;
        
        for (int i = 0; i < count; i++)
        {
            var open = price;
            var change = (decimal)(Random.NextDouble() - 0.5) * 2; // ±2% max change
            var close = open * (1 + change / 100);
            
            // Ensure high/low are realistic
            var high = Math.Max(open, close) * (1 + (decimal)Random.NextDouble() * 0.005m);
            var low = Math.Min(open, close) * (1 - (decimal)Random.NextDouble() * 0.005m);
            
            yield return new PhemexKlineItem
            {
                Symbol = symbol,
                Interval = interval,
                OpenTime = timestamp.ToUnixTimeMilliseconds(),
                CloseTime = timestamp.Add(intervalSpan).ToUnixTimeMilliseconds() - 1,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = Random.Next(100, 10000),
                QuoteVolume = Random.Next(1000000, 100000000),
                TradeCount = Random.Next(10, 1000)
            };
            
            price = close;
            timestamp = timestamp.Add(intervalSpan);
        }
    }

    public static PhemexOrderBook GenerateOrderBook(
        string symbol,
        int depth = 20,
        decimal? midPrice = null)
    {
        var (minPrice, maxPrice) = Prices.Ranges.ContainsKey(symbol) 
            ? Prices.Ranges[symbol] 
            : (1000m, 2000m);
        
        var mid = midPrice ?? (minPrice + maxPrice) / 2;
        var spread = mid * 0.0001m; // 0.01% spread
        
        var bids = new List<PhemexOrderBookEntry>();
        var asks = new List<PhemexOrderBookEntry>();
        
        // Generate bids (descending from mid - spread/2)
        var bidPrice = mid - spread / 2;
        for (int i = 0; i < depth; i++)
        {
            bids.Add(new PhemexOrderBookEntry
            {
                Price = bidPrice,
                Quantity = Random.Next(1, 1000),
                Count = Random.Next(1, 10)
            });
            bidPrice -= Random.Next(1, 10) * 0.01m;
        }
        
        // Generate asks (ascending from mid + spread/2)
        var askPrice = mid + spread / 2;
        for (int i = 0; i < depth; i++)
        {
            asks.Add(new PhemexOrderBookEntry
            {
                Price = askPrice,
                Quantity = Random.Next(1, 1000),
                Count = Random.Next(1, 10)
            });
            askPrice += Random.Next(1, 10) * 0.01m;
        }
        
        return new PhemexOrderBook
        {
            Symbol = symbol,
            Timestamp = DateTimeOffset.UtcNow,
            Bids = bids,
            Asks = asks
        };
    }

    public static PhemexBalance GenerateBalance(
        string currency = "USD",
        decimal? balance = null,
        decimal? available = null)
    {
        var totalBalance = balance ?? Random.Next(1000, 100000);
        var availableBalance = available ?? totalBalance * 0.8m;
        
        return new PhemexBalance
        {
            Currency = currency,
            Balance = totalBalance,
            Available = availableBalance,
            Frozen = totalBalance - availableBalance,
            BtcValue = currency == "BTC" ? totalBalance : totalBalance / 45000m
        };
    }

    public static PhemexPosition GeneratePosition(
        string symbol,
        string side = "Long",
        decimal? size = null,
        decimal? entryPrice = null)
    {
        var (minPrice, maxPrice) = Prices.Ranges.ContainsKey(symbol) 
            ? Prices.Ranges[symbol] 
            : (1000m, 2000m);
        
        var entry = entryPrice ?? (minPrice + maxPrice) / 2;
        var currentPrice = entry * (1 + (decimal)(Random.NextDouble() - 0.5) * 0.1m);
        var positionSize = size ?? Random.Next(1, 100);
        var pnl = (currentPrice - entry) * positionSize * (side == "Long" ? 1 : -1);
        
        return new PhemexPosition
        {
            Symbol = symbol,
            Side = side,
            Size = positionSize,
            EntryPrice = entry,
            MarkPrice = currentPrice,
            LiquidationPrice = side == "Long" 
                ? entry * 0.8m 
                : entry * 1.2m,
            UnrealizedPnl = pnl,
            RealizedPnl = 0,
            Margin = positionSize * entry * 0.01m, // 1% margin
            MarginRatio = 0.01m,
            PositionId = Guid.NewGuid().ToString()
        };
    }

    public static PhemexOrder GenerateOrder(
        string symbol,
        string side = "Buy",
        string orderType = "Limit",
        decimal? price = null,
        decimal? quantity = null)
    {
        var (minPrice, maxPrice) = Prices.Ranges.ContainsKey(symbol) 
            ? Prices.Ranges[symbol] 
            : (1000m, 2000m);
        
        var orderPrice = price ?? (minPrice + maxPrice) / 2;
        var orderQty = quantity ?? Random.Next(1, 100);
        
        return new PhemexOrder
        {
            OrderId = Guid.NewGuid().ToString(),
            Symbol = symbol,
            Side = side,
            OrderType = orderType,
            Price = orderType == "Limit" ? orderPrice : null,
            Quantity = orderQty,
            FilledQuantity = 0,
            Status = "New",
            CreateTime = DateTimeOffset.UtcNow,
            UpdateTime = DateTimeOffset.UtcNow,
            TimeInForce = "GTC",
            ReduceOnly = false,
            PostOnly = orderType == "Limit" && Random.Next(2) == 0,
            Hidden = false
        };
    }

    private static TimeSpan ParseInterval(string interval)
    {
        return interval.ToLower() switch
        {
            "1m" => TimeSpan.FromMinutes(1),
            "5m" => TimeSpan.FromMinutes(5),
            "15m" => TimeSpan.FromMinutes(15),
            "30m" => TimeSpan.FromMinutes(30),
            "1h" => TimeSpan.FromHours(1),
            "4h" => TimeSpan.FromHours(4),
            "1d" => TimeSpan.FromDays(1),
            _ => TimeSpan.FromMinutes(1)
        };
    }
}

// Data models for Phemex
public class PhemexTick
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public TradeSide Side { get; set; }
    public string TradeId { get; set; } = string.Empty;
}

public class PhemexKlineItem
{
    public string Symbol { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public long OpenTime { get; set; }
    public long CloseTime { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal QuoteVolume { get; set; }
    public int TradeCount { get; set; }
}

public class PhemexOrderBook
{
    public string Symbol { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public List<PhemexOrderBookEntry> Bids { get; set; } = new();
    public List<PhemexOrderBookEntry> Asks { get; set; } = new();
}

public class PhemexOrderBookEntry
{
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public int Count { get; set; }
}

public class PhemexBalance
{
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal Available { get; set; }
    public decimal Frozen { get; set; }
    public decimal BtcValue { get; set; }
}

public class PhemexPosition
{
    public string PositionId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public decimal Size { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal MarkPrice { get; set; }
    public decimal LiquidationPrice { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public decimal RealizedPnl { get; set; }
    public decimal Margin { get; set; }
    public decimal MarginRatio { get; set; }
}

public class PhemexOrder
{
    public string OrderId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal FilledQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreateTime { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
    public string TimeInForce { get; set; } = string.Empty;
    public bool ReduceOnly { get; set; }
    public bool PostOnly { get; set; }
    public bool Hidden { get; set; }
}

public enum TradeSide
{
    Buy,
    Sell
}