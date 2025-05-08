using ReactiveUI;
using ReactiveUI.SourceGenerators;

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

public partial class PBotHarness : ReactiveObject
{
    public bool TicksEnabled() => Features.HasFlag(BotHarnessFeatures.Ticks);
    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => ExchangeSymbol == null || TimeFrame == null ? null : new ExchangeSymbolTimeFrame(ExchangeSymbol.Exchange, ExchangeSymbol.ExchangeArea, ExchangeSymbol.Symbol, TimeFrame);

    //public Type? PBotType { get; set; }
    [Reactive]
    private Type? _pBotType;

    #region Time

    public TimeFrame? TimeFrame { get; set; }
    public string? TimeFrameString { get => TimeFrame; set => TimeFrame = value; }

    public virtual TimeFrame? EffectiveTimeFrame => TimeFrame;

    //public TimeFrame TimeFrame => PBot.TimeFrame;

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

    public string? Exchange { get => ExchangeSymbol?.Exchange; set => ExchangeSymbol = new ExchangeSymbol(value!, ExchangeSymbol?.ExchangeArea!, ExchangeSymbol?.Symbol!); }
    public string? ExchangeArea { get => ExchangeSymbol?.ExchangeArea; set => ExchangeSymbol = new ExchangeSymbol(ExchangeSymbol?.Exchange!, value!, ExchangeSymbol?.Symbol!); }
    public string? Symbol { get => ExchangeSymbol?.Symbol; set => ExchangeSymbol = new ExchangeSymbol(ExchangeSymbol?.Exchange!, ExchangeSymbol?.ExchangeArea!, value!); }

    #endregion

    /// <summary>
    /// null if ExchangeSymbol is set instead.  Order is important, with the first symbol typically being the primary one.
    /// </summary>
    public ExchangeSymbol[]? ExchangeSymbols { get; set; }

    #endregion

}

public static class DateCoercion
{
    public static DateTimeOffset Coerce(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return default;
        var dt = dateTime.Value;
        if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return dt;
    }
}

// OPTIMIZE: change set properties to init?
public partial class PBacktestBatchTask2 : PBotHarness //: IPBacktestBatchTask2
{

    #region Time
    
    public DateTimeOffset Start { get; set; }
    public DateTime? StartDateTime { get => Start.DateTime; set => Start = Coerce(value); }
    public DateTimeOffset EndExclusive { get; set; }
    public DateTime? EndExclusiveDateTime { get => EndExclusive.DateTime; set => EndExclusive = Coerce(value); }

    public static DateTimeOffset Coerce(DateTime? dateTime) => DateCoercion.Coerce(dateTime);
    
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
    public PBacktestTask2(PMultiBacktest pMultiBacktest)
    {
        if(!pMultiBacktest.IsValid)
        {
            throw new ArgumentException("All parameters must be set");
        }

        Start = pMultiBacktest.Start!.Value;
        EndExclusive = pMultiBacktest.EndExclusive!.Value;
        ExchangeSymbol = pMultiBacktest.ExchangeSymbolTimeFrame;
        //ExchangeSymbols = pMultiBacktest.ExchangeSymbols; // FUTURE
        Features = pMultiBacktest.Features;
        TimeFrame = pMultiBacktest.ExchangeSymbolTimeFrame!.TimeFrame;
        //ShortChunks = pMultiBacktest.ShortChunks; // OLD

    }

    #region Bot

    //public Type BotType => botType ?? PBot?.GetType()!;
    //private Type? botType;
    public IPTimeFrameBot2? PBot { get; set; }

    #endregion

    public override TimeFrame? EffectiveTimeFrame => TimeFrame ?? PBot?.TimeFrame;

    public Action? OnFinished { get; internal set; }
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
