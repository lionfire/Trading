namespace LionFire.Trading.Automation;

// REFACTOR: Eliminate IPBacktestTask2 and use PBacktestTask2 instead
//public interface PBacktestTask2
//    : PBacktestBatchTask2x
//{
//    IPTimeFrameBot2? PBot { get; }
//    //IPBacktestBatchTask2 PBacktestBatchTask { get; }  // TODO: Reference IPBacktestBatchTask2 instead of inheriting
//}


//public interface IPBacktestBatchTask2 // REFACTOR: Eliminate IPBacktestBatchTask2 and use PBacktestBatchTask2 instead
//{
//    Type? PBotType { get; }

//    BotHarnessFeatures Features { get; }

//    ExchangeSymbol? ExchangeSymbol { get; }
//    ExchangeSymbol[]? ExchangeSymbols { get; set; }
//    TimeFrame? TimeFrame { get; }
//    TimeFrame? EffectiveTimeFrame { get; }


//    DateTimeOffset Start { get; }
//    DateTimeOffset EndExclusive { get; }
//}

// OPTIMIZE: change set properties to init?
public class PBacktestBatchTask2 //: IPBacktestBatchTask2
{
    public bool TicksEnabled() => Features.HasFlag(BotHarnessFeatures.Ticks);
    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => ExchangeSymbol == null || TimeFrame == null ? null : new ExchangeSymbolTimeFrame(ExchangeSymbol.Exchange, ExchangeSymbol.ExchangeArea, ExchangeSymbol.Symbol, TimeFrame);

    public Type? PBotType { get; set; }

    #region Time

    public TimeFrame? TimeFrame { get; set; }
    public string TimeFrameString { get => TimeFrame; set => TimeFrame = value; }

    public virtual TimeFrame? EffectiveTimeFrame => TimeFrame;

    //public TimeFrame TimeFrame => PBot.TimeFrame;
    public DateTimeOffset Start { get; set; }
    public DateTime? StartDateTime { get=> Start.DateTime; set => Start = value ?? default; }
    public DateTimeOffset EndExclusive { get; set; }
    public DateTime? EndExclusiveDateTime { get => EndExclusive.DateTime; set => EndExclusive = value ?? default; }

    #endregion

    #region Features

    public BotHarnessFeatures Features { get; set; } = BotHarnessFeatures.Bars;

    #endregion

    #region ExchangeSymbol(s) discriminated union

    // OPTIMIZATION idea: always use an ExchangeSymbol field for first element of ExchangeSymbols

    /// <summary>
    /// null if ExchangeSymbols is set instead
    /// </summary>
    public ExchangeSymbol? ExchangeSymbol { get; set; }


    #region REVIEW - Unimmutabilizing properties

    public string Exchange { get => ExchangeSymbol.Exchange; set => ExchangeSymbol = new ExchangeSymbol(value, ExchangeSymbol.ExchangeArea, ExchangeSymbol.Symbol); }
    public string ExchangeArea { get => ExchangeSymbol.ExchangeArea; set => ExchangeSymbol = new ExchangeSymbol(ExchangeSymbol.Exchange, value, ExchangeSymbol.Symbol); }
    public string Symbol { get => ExchangeSymbol.Symbol; set => ExchangeSymbol = new ExchangeSymbol(ExchangeSymbol.Exchange, ExchangeSymbol.ExchangeArea, value); }

    #endregion

    /// <summary>
    /// null if ExchangeSymbol is set instead.  Order is important, with the first symbol typically being the primary one.
    /// </summary>
    public ExchangeSymbol[]? ExchangeSymbols { get; set; }

    #endregion

    #region Performance tuning

    public bool ShortChunks { get; set; }

    #endregion

}

public class PBacktestTask2 : PBacktestBatchTask2 
{

    //public static PBacktestTask2<PBot> Create<PBot>(PBot bot, TimeFrame timeFrame, DateTimeOffset start, DateTimeOffset endExclusive)
    //    where PBot : IPTimeFrameBot2
    //    => new PBacktestTask2<PBot>
    //    {
    //        Bot = bot,
    //        TimeFrame = timeFrame,
    //        Start = start,
    //        EndExclusive = endExclusive,
    //    };


    public PBacktestTask2() { }
    public PBacktestTask2(PBacktestBatchTask2 pBacktestBatchTask2)
    {
        Start = pBacktestBatchTask2.Start;
        EndExclusive = pBacktestBatchTask2.EndExclusive;
        ExchangeSymbol = pBacktestBatchTask2.ExchangeSymbol;
        ExchangeSymbols = pBacktestBatchTask2.ExchangeSymbols;
        Features = pBacktestBatchTask2.Features;
        TimeFrame = pBacktestBatchTask2.TimeFrame;
        ShortChunks = pBacktestBatchTask2.ShortChunks;

    }

    #region Bot

    //public Type BotType => botType ?? PBot?.GetType()!;
    //private Type? botType;
    public IPTimeFrameBot2? PBot { get; set; }

    #endregion

    public override TimeFrame? EffectiveTimeFrame => TimeFrame ?? PBot?.TimeFrame;

    public Action OnFinished { get; internal set; }
}

public sealed class PBacktestTask2<TPBot> : PBacktestTask2 
    where TPBot : IPTimeFrameBot2
{

    #region Lifecycle

    public PBacktestTask2() { }
    public PBacktestTask2(IBacktestBatchJob j)
    {
    }

    #endregion

    #region Bot

    //public TPBot Bot { get => (TPBot)base.PBot!; set => base.PBot = value; }
    //IPTimeFrameBot2 IPBacktestTask2.PBot => Bot;

    #endregion


}
