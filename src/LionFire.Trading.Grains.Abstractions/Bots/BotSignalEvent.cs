using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Lightweight event emitted by <see cref="IRealtimeBotHarnessG"/> when a bot's
/// position state changes (open/close/flip). Consumed by <see cref="IBotSignalObserver"/>.
/// </summary>
/// <remarks>
/// This is a transport DTO kept in grains abstractions to avoid coupling to the
/// signal-aggregator module. The <c>SignalCollectorG</c> maps these events to the
/// full <c>BotSignal</c> model by attaching deployment metadata (ADWT, win rate, etc.).
/// </remarks>
[GenerateSerializer]
[Immutable]
public sealed record BotSignalEvent
{
    /// <summary>
    /// The bot harness grain ID that emitted this signal.
    /// </summary>
    [Id(0)]
    public required string BotId { get; init; }

    /// <summary>
    /// The trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [Id(1)]
    public required string Symbol { get; init; }

    /// <summary>
    /// The exchange (e.g., "Binance").
    /// </summary>
    [Id(2)]
    public string? Exchange { get; init; }

    /// <summary>
    /// The exchange area (e.g., "Futures").
    /// </summary>
    [Id(3)]
    public string? ExchangeArea { get; init; }

    /// <summary>
    /// The timeframe the bot operates on.
    /// </summary>
    [Id(4)]
    public string? TimeFrame { get; init; }

    /// <summary>
    /// When the signal was generated (bar open time or current UTC).
    /// </summary>
    [Id(5)]
    public required DateTime SignalTime { get; init; }

    /// <summary>
    /// The type of position change.
    /// </summary>
    [Id(6)]
    public required BotSignalDirection Direction { get; init; }

    /// <summary>
    /// Signal strength from -1.0 (max bearish) to +1.0 (max bullish).
    /// 0.0 indicates a close/flat signal.
    /// </summary>
    [Id(7)]
    public double SignalStrength { get; init; }

    /// <summary>
    /// The price at which the position was opened or closed.
    /// </summary>
    [Id(8)]
    public double? Price { get; init; }

    /// <summary>
    /// The bot type name (e.g., "LorentzianDynamicBot").
    /// </summary>
    [Id(9)]
    public string? BotTypeName { get; init; }
}

/// <summary>
/// Direction of the signal emitted by a bot.
/// </summary>
[GenerateSerializer]
public enum BotSignalDirection
{
    /// <summary>No signal / flat.</summary>
    Flat = 0,

    /// <summary>Bot opened or holds a long position.</summary>
    Long = 1,

    /// <summary>Bot opened or holds a short position.</summary>
    Short = -1,

    /// <summary>Bot closed its position (was long or short, now flat).</summary>
    Close = 2,
}
