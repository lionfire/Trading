using System;

namespace LionFire.Trading.Feeds.Models;

public record MarketDataSnapshot
{
    public required string Exchange { get; init; }
    public required string Symbol { get; init; }
    public required DateTime Timestamp { get; init; }
    
    // CVD (Cumulative Volume Delta) data
    public decimal CumulativeVolumeDelta { get; init; }
    public decimal LastTradePrice { get; init; }
    public decimal LastTradeVolume { get; init; }
    public bool LastTradeIsBuy { get; init; }
    
    // Bid/Ask prices
    public decimal BidPrice { get; init; }
    public decimal AskPrice { get; init; }
    
    // Order book depth at various percentage levels from mid price
    public OrderBookDepth? OrderBookDepth { get; init; }
    
    // Trigger type that caused this snapshot
    public SnapshotTrigger Trigger { get; init; }
}

public record OrderBookDepth
{
    // Depth at various percentage levels from mid price
    public DepthLevel Depth01Percent { get; init; } = new();  // 0.1%
    public DepthLevel Depth025Percent { get; init; } = new(); // 0.25%
    public DepthLevel Depth05Percent { get; init; } = new();  // 0.5%
    public DepthLevel Depth075Percent { get; init; } = new(); // 0.75%
    public DepthLevel Depth1Percent { get; init; } = new();   // 1%
    public DepthLevel Depth2Percent { get; init; } = new();   // 2%
}

public record DepthLevel
{
    public decimal BidVolume { get; init; }
    public decimal AskVolume { get; init; }
    public decimal BidPrice { get; init; }
    public decimal AskPrice { get; init; }
}

public enum SnapshotTrigger
{
    Trade,
    OrderBookChange,
    Timer,
    Manual
}