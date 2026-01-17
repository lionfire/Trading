using Orleans;

namespace LionFire.Trading.Streaming;

/// <summary>
/// Represents a single price level in an orderbook.
/// </summary>
/// <remarks>
/// Bids are sorted descending (highest first), asks are sorted ascending (lowest first).
/// A quantity of 0 indicates this price level should be removed.
/// </remarks>
[GenerateSerializer]
[Alias("PriceLevel")]
public readonly record struct PriceLevel : IComparable<PriceLevel>
{
    /// <summary>
    /// The price at this level.
    /// </summary>
    [Id(0)]
    public required decimal Price { get; init; }

    /// <summary>
    /// The total quantity at this price level.
    /// </summary>
    [Id(1)]
    public required decimal Quantity { get; init; }

    /// <summary>
    /// Whether this represents a removal (quantity is zero).
    /// </summary>
    public bool IsZero => Quantity == 0;

    /// <summary>
    /// Compares price levels by price for sorting.
    /// </summary>
    public int CompareTo(PriceLevel other) => Price.CompareTo(other.Price);
}

/// <summary>
/// Type of update to a price level.
/// </summary>
public enum PriceLevelUpdateType
{
    /// <summary>New price level added.</summary>
    Add,

    /// <summary>Existing price level quantity updated.</summary>
    Update,

    /// <summary>Price level removed (quantity = 0).</summary>
    Remove
}

/// <summary>
/// Represents an update to a single price level.
/// </summary>
[GenerateSerializer]
[Alias("PriceLevelUpdate")]
public readonly record struct PriceLevelUpdate
{
    /// <summary>
    /// The price being updated.
    /// </summary>
    [Id(0)]
    public required decimal Price { get; init; }

    /// <summary>
    /// The new quantity (0 means remove this level).
    /// </summary>
    [Id(1)]
    public required decimal Quantity { get; init; }

    /// <summary>
    /// The type of update.
    /// </summary>
    [Id(2)]
    public required PriceLevelUpdateType Type { get; init; }

    /// <summary>
    /// Creates an add update for a new price level.
    /// </summary>
    public static PriceLevelUpdate Added(decimal price, decimal quantity) => new()
    {
        Price = price,
        Quantity = quantity,
        Type = PriceLevelUpdateType.Add
    };

    /// <summary>
    /// Creates an update for an existing price level.
    /// </summary>
    public static PriceLevelUpdate Updated(decimal price, decimal quantity) => new()
    {
        Price = price,
        Quantity = quantity,
        Type = PriceLevelUpdateType.Update
    };

    /// <summary>
    /// Creates a removal update for a price level.
    /// </summary>
    public static PriceLevelUpdate Removed(decimal price) => new()
    {
        Price = price,
        Quantity = 0,
        Type = PriceLevelUpdateType.Remove
    };
}

/// <summary>
/// Complete snapshot of an orderbook at a point in time.
/// </summary>
/// <remarks>
/// Snapshots are delivered:
/// - On initial subscription
/// - After sequence gap detection
/// - On reconnection recovery
/// </remarks>
[GenerateSerializer]
[Alias("OrderBookSnapshot")]
public sealed record OrderBookSnapshot
{
    /// <summary>
    /// The trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [Id(0)]
    public required string Symbol { get; init; }

    /// <summary>
    /// The last update ID in this snapshot, used for sequence tracking.
    /// </summary>
    [Id(1)]
    public required long LastUpdateId { get; init; }

    /// <summary>
    /// Bid price levels, sorted descending (highest first).
    /// </summary>
    [Id(2)]
    public required IReadOnlyList<PriceLevel> Bids { get; init; }

    /// <summary>
    /// Ask price levels, sorted ascending (lowest first).
    /// </summary>
    [Id(3)]
    public required IReadOnlyList<PriceLevel> Asks { get; init; }

    /// <summary>
    /// When this snapshot was created.
    /// </summary>
    [Id(4)]
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Maximum depth in this snapshot (max of bids and asks count).
    /// </summary>
    public int Depth => Math.Max(Bids.Count, Asks.Count);

    /// <summary>
    /// Best (highest) bid price, or null if no bids.
    /// </summary>
    public decimal? BestBid => Bids.Count > 0 ? Bids[0].Price : null;

    /// <summary>
    /// Best (lowest) ask price, or null if no asks.
    /// </summary>
    public decimal? BestAsk => Asks.Count > 0 ? Asks[0].Price : null;

    /// <summary>
    /// Mid price between best bid and ask.
    /// </summary>
    public decimal? MidPrice => BestBid.HasValue && BestAsk.HasValue
        ? (BestBid.Value + BestAsk.Value) / 2
        : null;

    /// <summary>
    /// Spread between best ask and best bid.
    /// </summary>
    public decimal? Spread => BestBid.HasValue && BestAsk.HasValue
        ? BestAsk.Value - BestBid.Value
        : null;

    /// <summary>
    /// Spread in basis points (1 bp = 0.01%).
    /// </summary>
    public decimal? SpreadBps => MidPrice.HasValue && MidPrice.Value > 0 && Spread.HasValue
        ? Spread.Value / MidPrice.Value * 10000
        : null;
}

/// <summary>
/// Incremental orderbook update containing changed price levels.
/// </summary>
/// <remarks>
/// Deltas should be applied in sequence using FirstUpdateId/LastUpdateId.
/// For Binance: first delta must have FirstUpdateId <= snapshot.LastUpdateId+1 AND LastUpdateId >= snapshot.LastUpdateId+1
/// </remarks>
[GenerateSerializer]
[Alias("OrderBookDelta")]
public sealed record OrderBookDelta
{
    /// <summary>
    /// The trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [Id(0)]
    public required string Symbol { get; init; }

    /// <summary>
    /// First update ID in this delta batch.
    /// </summary>
    [Id(1)]
    public required long FirstUpdateId { get; init; }

    /// <summary>
    /// Last update ID in this delta batch.
    /// </summary>
    [Id(2)]
    public required long LastUpdateId { get; init; }

    /// <summary>
    /// Previous update ID (for Binance sequence validation).
    /// </summary>
    [Id(3)]
    public long? PreviousUpdateId { get; init; }

    /// <summary>
    /// Changes to bid price levels.
    /// </summary>
    [Id(4)]
    public required IReadOnlyList<PriceLevelUpdate> BidUpdates { get; init; }

    /// <summary>
    /// Changes to ask price levels.
    /// </summary>
    [Id(5)]
    public required IReadOnlyList<PriceLevelUpdate> AskUpdates { get; init; }

    /// <summary>
    /// When this delta was received.
    /// </summary>
    [Id(6)]
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Total number of updates in this delta.
    /// </summary>
    public int UpdateCount => BidUpdates.Count + AskUpdates.Count;
}

/// <summary>
/// State of an L2 orderbook scraper.
/// </summary>
public enum OrderBookScraperState
{
    /// <summary>State unknown.</summary>
    Unknown,

    /// <summary>Scraper is not active.</summary>
    Inactive,

    /// <summary>Scraper is fetching initial snapshot.</summary>
    FetchingSnapshot,

    /// <summary>Scraper is synchronizing snapshot with deltas.</summary>
    Synchronizing,

    /// <summary>Scraper is active and streaming data.</summary>
    Active,

    /// <summary>Scraper is reconnecting after disconnection.</summary>
    Reconnecting,

    /// <summary>Scraper has failed and is not streaming.</summary>
    Failed
}
