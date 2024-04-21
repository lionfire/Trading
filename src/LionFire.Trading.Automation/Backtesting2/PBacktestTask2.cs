namespace LionFire.Trading.Automation;

public class PBacktestTask2<PBot, TBot>
     //where PBot : ITemplate<TBot>
{
    public bool TicksEnabled { get; set; }

    /// <summary>
    /// Refuse to run if order book info is not available for the symbol being traded
    /// </summary>
    public bool OrderBook { get; set; }

    public required SymbolBarsRange SymbolBarsRange { get; init; }

    /// <summary>
    /// Primary symbol being traded.
    /// (Some bots may trade multiple symbols, in which case this symbol may be unused or superfluous.)
    /// </summary>
    public string Symbol => SymbolBarsRange.Symbol;
    public TimeFrame TimeFrame => SymbolBarsRange.TimeFrame;
    public DateTimeOffset Start => SymbolBarsRange.Start;
    public DateTimeOffset EndExclusive => SymbolBarsRange.EndExclusive;
}
