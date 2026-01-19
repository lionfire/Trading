using Orleans;

namespace LionFire.Trading.Grains.User;

/// <summary>
/// Preferences for optimization settings per bot type.
/// Stores exchange, symbol, timeframe, date range, and optimization parameters.
/// </summary>
[GenerateSerializer]
[Alias("bot-optimization-preferences")]
public class BotOptimizationPreferences
{
    /// <summary>
    /// Exchange identifier (e.g., "Binance", "Bitfinex").
    /// </summary>
    [Id(0)]
    public string? Exchange { get; set; }

    /// <summary>
    /// Exchange area (e.g., "futures", "spot").
    /// </summary>
    [Id(1)]
    public string? ExchangeArea { get; set; }

    /// <summary>
    /// Trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [Id(2)]
    public string? Symbol { get; set; }

    /// <summary>
    /// Time frame string (e.g., "m1", "h1", "d1").
    /// </summary>
    [Id(3)]
    public string? TimeFrame { get; set; }

    /// <summary>
    /// Start date for backtesting/optimization.
    /// </summary>
    [Id(4)]
    public DateOnly? Start { get; set; }

    /// <summary>
    /// End date (exclusive) for backtesting/optimization.
    /// </summary>
    [Id(5)]
    public DateOnly? End { get; set; }

    /// <summary>
    /// Maximum number of backtests to run during optimization.
    /// </summary>
    [Id(6)]
    public long? MaxBacktests { get; set; }

    /// <summary>
    /// Minimum parameter priority for optimization.
    /// </summary>
    [Id(7)]
    public int? MinParameterPriority { get; set; }

    /// <summary>
    /// Timestamp when preferences were last modified.
    /// </summary>
    [Id(8)]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
