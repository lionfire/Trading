namespace LionFire.Trading.Automation;

public interface IPBacktestTask2
{

    IPTimeFrameBot2 Bot { get; }

    BotHarnessFeatures Features { get; }
    bool TicksEnabled() => Features.HasFlag(BotHarnessFeatures.Ticks);

    ExchangeSymbol? ExchangeSymbol { get; }
    ExchangeSymbol[]? ExchangeSymbols { get; set; }
    TimeFrame TimeFrame { get; }
    DateTimeOffset Start { get; }
    DateTimeOffset EndExclusive { get; }

    /// <summary>
    /// Backtest optimization: use short Chunks instead of long
    /// </summary>
    bool ShortChunks { get; }

}

[Flags]
public enum BotHarnessFeatures
{
    Unspecified = 0,
    Bars = 1 << 0,
    Ticks = 1 << 1,
    /// <summary>
    /// Refuse to run if order book info is not available for the symbol being traded
    /// </summary>
    OrderBook = 1 << 2,
}

public class PBacktestTask2<PBot> : IPBacktestTask2
    where PBot : IPTimeFrameBot2
{
    #region Bot

    public required PBot Bot { get; init; }
    IPTimeFrameBot2 IPBacktestTask2.Bot => Bot;

    #endregion

    #region Time

    public TimeFrame TimeFrame => Bot.TimeFrame;
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset EndExclusive { get; init; }

    #endregion

    #region Features

    public BotHarnessFeatures Features { get; set; } = BotHarnessFeatures.Bars;

    #endregion

    #region ExchangeSymbol(s) discriminated union

    // OPTIMIZATION idea: always use an ExchangeSymbol field for first element of ExchangeSymbols

    /// <summary>
    /// null if ExchangeSymbols is set instead
    /// </summary>
    public ExchangeSymbol? ExchangeSymbol { get; init; }

    /// <summary>
    /// null if ExchangeSymbol is set instead.  Order is important, with the first symbol typically being the primary one.
    /// </summary>
    public ExchangeSymbol[]? ExchangeSymbols { get; set; }

    #endregion

    #region Performance tuning

    public bool ShortChunks { get; init; }

    #endregion
}
