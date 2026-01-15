using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Chandelier Exit indicator interface.
/// </summary>
/// <remarks>
/// The Chandelier Exit is a volatility-based trailing stop system developed by Charles Le Beau.
/// It uses ATR (Average True Range) to set trailing stop-loss levels that adapt to market volatility.
///
/// Formula:
/// - Chandelier Exit Long = Highest High (N) - ATR(N) × Multiplier
/// - Chandelier Exit Short = Lowest Low (N) + ATR(N) × Multiplier
///
/// The name "Chandelier" comes from its visual appearance - it "hangs" from the ceiling
/// (highest high) like a chandelier.
///
/// Available implementations:
/// - ChandelierExit_QC: QuantConnect implementation (default, stable)
/// - ChandelierExit_FP: First-party implementation (custom features, optimized for streaming)
///
/// Common parameters:
/// - Period: 22 (approximately one trading month)
/// - Multiplier: 3.0
/// </remarks>
public interface IChandelierExit<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for ATR and highest/lowest calculations
    /// </summary>
    int Period { get; }

    /// <summary>
    /// The multiplier applied to ATR for band calculation
    /// </summary>
    TOutput AtrMultiplier { get; }

    /// <summary>
    /// Chandelier Exit for long positions: HighestHigh - ATR × Multiplier
    /// Use as a trailing stop for long positions (exit when price falls below)
    /// </summary>
    TOutput ExitLong { get; }

    /// <summary>
    /// Chandelier Exit for short positions: LowestLow + ATR × Multiplier
    /// Use as a trailing stop for short positions (exit when price rises above)
    /// </summary>
    TOutput ExitShort { get; }

    /// <summary>
    /// Current ATR value used in the calculation
    /// </summary>
    TOutput CurrentATR { get; }

    /// <summary>
    /// Highest high over the lookback period
    /// </summary>
    TOutput HighestHigh { get; }

    /// <summary>
    /// Lowest low over the lookback period
    /// </summary>
    TOutput LowestLow { get; }
}
