using LionFire.Execution;
using LionFire.ExtensionMethods;
using LionFire.ExtensionMethods.Cloning;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Automation;

public abstract class BotHarnessBase
    //: IHasInputMappings
{
    
    #region Parent

    public abstract ISimContext ISimContext { get; }

    #endregion

    protected IBot2 InstantiateBot<TPrecision>(MultiSimContext multiSimContext, IPBot2? pBot)
    where TPrecision : struct, INumber<TPrecision>
    {
        ArgumentNullException.ThrowIfNull(pBot);

        // Use ActivatorUtilities to support DI constructor injection (e.g., ILogger)
        var bot = (IBot2<TPrecision>)(ActivatorUtilities.CreateInstance(multiSimContext.ServiceProvider, pBot.MaterializedType)
            ?? throw new Exception("Failed to create bot: " + pBot.MaterializedType));

        bot.Parameters = pBot;

        if (bot is IBarsBot<TPrecision> barsBot)
        {
            barsBot.Parameters.ExchangeSymbolTimeFrame ??= multiSimContext.Parameters.ExchangeSymbolTimeFrame ?? throw new ArgumentNullException(nameof(barsBot.Parameters.ExchangeSymbolTimeFrame));

            ArgumentNullException.ThrowIfNull(barsBot.Parameters.ExchangeSymbolTimeFrame?.Symbol);

            if (pBot is IPRequiresInitBeforeUse requiresInit) { requiresInit.Init(); }
        }

        return bot;
    }



}


/// <summary>
/// Batch processing of multiple bots:
/// - InputEnumerators are enumerated in lock step
/// </summary>
/// <remarks>
/// REVIEW: This base class should maybe be combined with BacktestBatchTask2 
/// </remarks>
public abstract class BatchHarnessBase : BotHarnessBase, IMultiBacktestHarness
{

    #region Dependencies

    public IServiceProvider ServiceProvider => MultiSimContext.ServiceProvider;

    #endregion

    #region Parents

    public abstract MultiSimContext MultiSimContext { get; }

    protected abstract ISimContext GetSimContext();

    #endregion

    #region Parameters

    #region Derived

    // Must match across all parameters

    public TimeFrame TimeFrame => MultiSimContext.Parameters.DefaultTimeFrame ?? throw new ArgumentNullException(nameof(TimeFrame));
    public DateTimeOffset InputStart { get; }
    public DateTimeOffset Start => MultiSimContext.Parameters.Start;
    public DateTimeOffset EndExclusive => MultiSimContext.Parameters.EndExclusive;

    #endregion

    #endregion

    #region Lifecycle

//    public BatchHarnessBase()
//    {
//        //var first = PBacktests.FirstOrDefault();
//        //if (first == null) throw new ArgumentException("batch empty");

//        //DefaultTimeFrame = first.DefaultTimeFrame ?? (first.PBot as IPTimeFrameBot2)?.DefaultTimeFrame ?? throw new ArgumentNullException($"Neither first {nameof(parameters)} nor first {nameof(first.PBot)} has a {nameof(first.DefaultTimeFrame)}");
//        //Start = first.Start;
//        //EndExclusive = first.EndExclusive;

//#if DEBUG
//        // Extra validation to make sure stray backtests ended up in a sim with the wrong parameters

//        //foreach (var p in PBacktests)
//        //{
//        //    if (p.DefaultTimeFrame != DefaultTimeFrame) throw new ArgumentException("DefaultTimeFrame mismatch");
//        //    if (p.Start != Start) throw new ArgumentException("Start mismatch");
//        //    if (p.EndExclusive != EndExclusive) throw new ArgumentException("EndExclusive mismatch");
//        //}
//#endif
//    }

    /// <summary>
    /// (No need to call this base method)
    /// </summary>
    /// <param name="p"></param>
    protected virtual void ValidateParameter(PBotWrapper p) { }

    protected virtual void OnFinishing()
    {
    }

    protected virtual ValueTask OnFinished()
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region State


    //#region Enumerators

    //public Dictionary<string, InputEnumeratorBase> InputEnumerators { get; } = new();

    //#endregion

    #endregion

    //#region Init

    //public IHistoricalTimeSeries Resolve(Type outputType, object source) => ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve(outputType, source);

    //#endregion 
}

