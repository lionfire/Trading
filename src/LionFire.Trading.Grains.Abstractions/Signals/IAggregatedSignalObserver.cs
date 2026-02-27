using LionFire.Trading.Grains.Bots;
using Orleans;

namespace LionFire.Trading.Grains.Signals;

/// <summary>
/// Observer interface for aggregated signal updates from <see cref="ISignalCollectorG"/>.
/// UI components implement this to receive real-time signal consensus updates.
/// </summary>
public interface IAggregatedSignalObserver : IGrainObserver
{
    /// <summary>
    /// Called when the aggregated signal for a symbol is updated.
    /// </summary>
    /// <param name="update">The latest aggregated signal state.</param>
    void OnAggregatedSignalUpdated(AggregatedSignalSnapshot update);
}

/// <summary>
/// Lightweight snapshot of the current aggregated signal state for a symbol.
/// Sent to <see cref="IAggregatedSignalObserver"/> consumers.
/// </summary>
[GenerateSerializer]
[Immutable]
public sealed record AggregatedSignalSnapshot
{
    /// <summary>
    /// The symbol this signal is for.
    /// </summary>
    [Id(0)]
    public required string Symbol { get; init; }

    /// <summary>
    /// Aggregated score from -1 (bearish consensus) to +1 (bullish consensus).
    /// </summary>
    [Id(1)]
    public double Score { get; init; }

    /// <summary>
    /// Number of bots actively contributing signals.
    /// </summary>
    [Id(2)]
    public int ActiveBotCount { get; init; }

    /// <summary>
    /// Effective signal count after correlation adjustment.
    /// </summary>
    [Id(3)]
    public double EffectiveSignalCount { get; init; }

    /// <summary>
    /// Confidence in the aggregated signal (0 to 1).
    /// </summary>
    [Id(4)]
    public double Confidence { get; init; }

    /// <summary>
    /// When this snapshot was computed.
    /// </summary>
    [Id(5)]
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Per-bot signal summaries.
    /// </summary>
    [Id(6)]
    public List<BotSignalSummary> BotSignals { get; init; } = [];

    /// <summary>
    /// Whether the consensus is bullish (score > threshold).
    /// </summary>
    public bool IsBullish => Score > 0.1;

    /// <summary>
    /// Whether the consensus is bearish (score < -threshold).
    /// </summary>
    public bool IsBearish => Score < -0.1;

    /// <summary>
    /// Whether the signal is neutral.
    /// </summary>
    public bool IsNeutral => !IsBullish && !IsBearish;
}

/// <summary>
/// Summary of a single bot's signal contribution within an aggregation.
/// </summary>
[GenerateSerializer]
[Immutable]
public sealed record BotSignalSummary
{
    [Id(0)] public required string BotId { get; init; }
    [Id(1)] public string? BotTypeName { get; init; }
    [Id(2)] public double SignalStrength { get; init; }
    [Id(3)] public BotSignalDirection Direction { get; init; }
    [Id(4)] public DateTime SignalTime { get; init; }
    [Id(5)] public double DecayedStrength { get; init; }
}
