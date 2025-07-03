using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace LionFire.Trading.Automation;

/// <summary>
/// Wraps PBot with the addition of an OnFinished func
/// </summary>
public partial class PBotWrapper //: PBotHarness
    : ReactiveObject
{
    #region Bot

    public IPTimeFrameBot2? PBot { get; set; }

    #endregion

    #region Event Handling

    public Action? OnFinished { get; internal set; }

    #endregion
}

//public sealed class PMultiBacktestItem<TPBot> : PBotWrapper 
//    where TPBot : IPTimeFrameBot2
//{

//    #region Lifecycle

//    public PMultiBacktestItem() { }
//    public PMultiBacktestItem(BacktestJob j)
//    {
//    }

//    #endregion

//    #region Bot

//    //public TPBot Bot { get => (TPBot)base.PBot!; set => base.PBot = value; }
//    //IPTimeFrameBot2 IPBacktestTask2.PBot => Bot;

//    #endregion

//}

// OLD

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
//    DefaultTimeFrame? DefaultTimeFrame { get; }
//    DefaultTimeFrame? EffectiveTimeFrame { get; }


//    DateTimeOffset Start { get; }
//    DateTimeOffset EndExclusive { get; }
//}

//public partial class PBotHarness : ReactiveObject
//{
//    //public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => ExchangeSymbol == null || DefaultTimeFrame == null ? null : new ExchangeSymbolTimeFrame(ExchangeSymbol.Exchange, ExchangeSymbol.Area, ExchangeSymbol.DefaultSymbol, DefaultTimeFrame);


//    #region Time

//    //public DefaultTimeFrame? DefaultTimeFrame { get; set; }
//    //public string? TimeFrameString { get => DefaultTimeFrame; set => DefaultTimeFrame = value; }

//    //public virtual DefaultTimeFrame? EffectiveTimeFrame => DefaultTimeFrame;

//    //public DefaultTimeFrame DefaultTimeFrame => PBot.DefaultTimeFrame;

//    #endregion

//    #region Features


//    #endregion

//    #region ExchangeSymbol(s) discriminated union

//    // OPTIMIZATION idea: always use an ExchangeSymbol field for first element of ExchangeSymbols

//    ///// <summary>
//    ///// null if ExchangeSymbols is set instead
//    ///// </summary>
//    //public ExchangeSymbol? ExchangeSymbol { get; set; }


//    #region REVIEW - Unimmutabilizing properties

//    //public string? Exchange { get => ExchangeSymbol?.Exchange; set => ExchangeSymbol = new ExchangeSymbol(value!, ExchangeSymbol?.Area!, ExchangeSymbol?.DefaultSymbol!); }
//    //public string? DefaultExchangeArea { get => ExchangeSymbol?.Area; set => ExchangeSymbol = new ExchangeSymbol(ExchangeSymbol?.Exchange!, value!, ExchangeSymbol?.DefaultSymbol!); }
//    //public string? DefaultSymbol { get => ExchangeSymbol?.DefaultSymbol; set => ExchangeSymbol = new ExchangeSymbol(ExchangeSymbol?.Exchange!, ExchangeSymbol?.Area!, value!); }

//    #endregion

//    /// <summary>
//    /// null if ExchangeSymbol is set instead.  Order is important, with the first symbol typically being the primary one.
//    /// </summary>
//    //public ExchangeSymbol[]? ExchangeSymbols { get; set; }

//    #endregion

//}
