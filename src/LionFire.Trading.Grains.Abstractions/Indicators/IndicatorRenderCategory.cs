namespace LionFire.Trading.Indicators.Grains;

/// <summary>
/// Categories for how indicators should be rendered in UI.
/// </summary>
public enum IndicatorRenderCategory
{
    /// <summary>
    /// Renders on the main price chart (SMA, EMA, Bollinger Bands).
    /// </summary>
    Overlay,

    /// <summary>
    /// Renders in a separate pane with defined range (RSI, Stochastic).
    /// </summary>
    Oscillator,

    /// <summary>
    /// Renders in a separate pane without defined range (MACD, Volume).
    /// </summary>
    Unbounded
}

#endregion
