using Orleans;

namespace LionFire.Trading.Streaming;

/// <summary>
/// Represents an aggregated trade (aggTrade) from an exchange.
/// Multiple individual trades at the same price and time are aggregated into a single record.
/// </summary>
/// <remarks>
/// This model captures essential trade data for:
/// - Price discovery and momentum analysis
/// - Volume tracking and accumulation detection
/// - Trade flow analysis (buyer vs seller initiated)
/// - Sequence tracking for gap detection
/// </remarks>
[GenerateSerializer]
[Alias("AggTrade")]
public readonly record struct AggTrade
{
    /// <summary>
    /// The unique aggregate trade ID from the exchange.
    /// Used for sequence tracking and gap detection.
    /// </summary>
    [Id(0)]
    public required long TradeId { get; init; }

    /// <summary>
    /// The trade execution price.
    /// </summary>
    [Id(1)]
    public required decimal Price { get; init; }

    /// <summary>
    /// The total quantity traded at this price.
    /// </summary>
    [Id(2)]
    public required decimal Quantity { get; init; }

    /// <summary>
    /// Whether the buyer was the market maker (true) or taker (false).
    /// - true: Sell order was the aggressor (bearish pressure)
    /// - false: Buy order was the aggressor (bullish pressure)
    /// </summary>
    [Id(3)]
    public required bool IsBuyerMaker { get; init; }

    /// <summary>
    /// The timestamp when the trade occurred on the exchange.
    /// </summary>
    [Id(4)]
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the trade direction indicator.
    /// Positive for buyer-initiated trades (buy pressure), negative for seller-initiated (sell pressure).
    /// </summary>
    public int Direction => IsBuyerMaker ? -1 : 1;

    /// <summary>
    /// Gets the signed quantity (positive for buys, negative for sells).
    /// </summary>
    public decimal SignedQuantity => IsBuyerMaker ? -Quantity : Quantity;
}

/// <summary>
/// A batch of aggregated trades for efficient network transmission and processing.
/// </summary>
/// <remarks>
/// Batching reduces Orleans messaging overhead by 10-100x compared to individual trade delivery.
/// The batch tracks sequence information for gap detection across batches.
/// </remarks>
[GenerateSerializer]
[Alias("AggTradeBatch")]
public sealed record AggTradeBatch
{
    /// <summary>
    /// The symbol this batch is for (e.g., "BTCUSDT").
    /// </summary>
    [Id(0)]
    public required string Symbol { get; init; }

    /// <summary>
    /// The trades in this batch, ordered by TradeId.
    /// </summary>
    [Id(1)]
    public required IReadOnlyList<AggTrade> Trades { get; init; }

    /// <summary>
    /// The first trade ID in this batch (for sequence tracking).
    /// </summary>
    [Id(2)]
    public required long SequenceStart { get; init; }

    /// <summary>
    /// The last trade ID in this batch (for sequence tracking).
    /// </summary>
    [Id(3)]
    public required long SequenceEnd { get; init; }

    /// <summary>
    /// When this batch was created (server-side timestamp).
    /// </summary>
    [Id(4)]
    public required DateTimeOffset BatchTime { get; init; }

    /// <summary>
    /// Gets the number of trades in this batch.
    /// </summary>
    public int Count => Trades.Count;

    /// <summary>
    /// Gets the total volume traded in this batch.
    /// </summary>
    public decimal TotalVolume => Trades.Sum(t => t.Quantity);

    /// <summary>
    /// Gets the total buy volume (taker buy = !IsBuyerMaker).
    /// </summary>
    public decimal BuyVolume => Trades.Where(t => !t.IsBuyerMaker).Sum(t => t.Quantity);

    /// <summary>
    /// Gets the total sell volume (taker sell = IsBuyerMaker).
    /// </summary>
    public decimal SellVolume => Trades.Where(t => t.IsBuyerMaker).Sum(t => t.Quantity);

    /// <summary>
    /// Gets the volume-weighted average price (VWAP) for this batch.
    /// </summary>
    public decimal VWAP
    {
        get
        {
            var totalValue = Trades.Sum(t => t.Price * t.Quantity);
            var totalQty = TotalVolume;
            return totalQty > 0 ? totalValue / totalQty : 0;
        }
    }

    /// <summary>
    /// Gets the time range spanned by trades in this batch.
    /// </summary>
    public TimeSpan TimeRange => Trades.Count > 1
        ? Trades[^1].Timestamp - Trades[0].Timestamp
        : TimeSpan.Zero;

    /// <summary>
    /// Creates an empty batch for the specified symbol.
    /// </summary>
    public static AggTradeBatch Empty(string symbol) => new()
    {
        Symbol = symbol,
        Trades = Array.Empty<AggTrade>(),
        SequenceStart = 0,
        SequenceEnd = 0,
        BatchTime = DateTimeOffset.UtcNow
    };
}
