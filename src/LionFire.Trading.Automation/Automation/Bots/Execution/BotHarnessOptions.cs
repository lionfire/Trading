namespace LionFire.Trading.Automation;

/// <summary>
/// Configuration options for bot harness creation and numeric type handling.
/// </summary>
public class BotHarnessOptions
{
    /// <summary>
    /// Default numeric type to use for live bots when no override is specified.
    /// Default: typeof(decimal) for maximum precision in live trading.
    /// </summary>
    public Type DefaultLiveNumericType { get; set; } = typeof(decimal);

    /// <summary>
    /// Default numeric type to use for backtesting when no override is specified.
    /// Default: null (use type from saved parameters for speed).
    /// </summary>
    // REVIEW: default to float when creating BotEntities?  And this only affects loading saved bot (backtest) parameters
    public Type? DefaultBacktestNumericType { get; set; } = null;

    /// <summary>
    /// Whether to log numeric type conversions for debugging.
    /// </summary>
    public bool LogTypeConversions { get; set; } = true;

    /// <summary>
    /// Whether to warn when type conversion may cause precision loss.
    /// </summary>
    public bool WarnOnPrecisionLoss { get; set; } = true;
}