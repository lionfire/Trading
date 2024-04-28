namespace LionFire.Trading.Automation;

public interface IPBacktestTask2
{
    bool TicksEnabled { get; }
    ExchangeSymbol? ExchangeSymbol { get; }
    ExchangeSymbol[]? ExchangeSymbols { get; set; }
    TimeFrame TimeFrame { get; }
    DateTimeOffset Start { get; }
    DateTimeOffset EndExclusive { get; }
}

[Flags]
public enum SimulationFeatures
{
    Unspecified = 0,
    Bars = 1 << 0,
    Ticks = 1 << 1,
    OrderBook = 1 << 2,
}

public class PBacktestTask2<PBot> : IPBacktestTask2
//where PBot : ITemplate<TBot>
{
    public SimulationFeatures SimulationFeatures { get; set; }

    public bool TicksEnabled { get; set; }

    /// <summary>
    /// Refuse to run if order book info is not available for the symbol being traded
    /// </summary>
    public bool OrderBook { get; set; }

    //public required SymbolBarsRange SymbolBarsRange { get; init; }

    #region ExchangeSymbol(s) discriminated union

    /// <summary>
    /// null if ExchangeSymbols is set instead
    /// </summary>
    public ExchangeSymbol? ExchangeSymbol { get; init; }

    /// <summary>
    /// null if ExchangeSymbol is set instead.  Order is important, with the first symbol typically being the primary one.
    /// </summary>
    public ExchangeSymbol[]? ExchangeSymbols { get; set; }

    #endregion

    public required TimeFrame TimeFrame { get; init; }
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset EndExclusive { get; init; }
}
