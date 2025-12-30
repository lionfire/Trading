namespace LionFire.Trading.Indicators.Grains;

/// <summary>
/// Metadata about an indicator grain.
/// </summary>
[GenerateSerializer]
public record IndicatorMetadata
{
    /// <summary>
    /// The indicator type name (e.g., "SMA", "RSI").
    /// </summary>
    [Id(0)]
    public required string TypeName { get; init; }

    /// <summary>
    /// The indicator category for UI rendering.
    /// </summary>
    [Id(1)]
    public required IndicatorRenderCategory Category { get; init; }

    /// <summary>
    /// The period/lookback for period-based indicators.
    /// </summary>
    [Id(2)]
    public int? Period { get; init; }

    /// <summary>
    /// For oscillators, the valid range (e.g., (0, 100) for RSI).
    /// </summary>
    [Id(3)]
    public (double Min, double Max)? OscillatorRange { get; init; }

    /// <summary>
    /// The full indicator key (e.g., "SMA(20)").
    /// </summary>
    [Id(4)]
    public required string IndicatorKey { get; init; }

    /// <summary>
    /// The exchange/symbol/timeframe this indicator is for.
    /// </summary>
    [Id(5)]
    public required string ExchangeSymbolTimeFrame { get; init; }
}

