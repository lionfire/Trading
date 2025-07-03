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
{
    protected IBot2 InstantiateBot<TPrecision>(IPBot2? pBot)
        where TPrecision : struct, INumber<TPrecision>
    {
        ArgumentNullException.ThrowIfNull(pBot);

        //var bot = ActivatorUtilities.CreateInstance(p.PBot.MaterializedType) // FUTURE

        var bot = (IBot2<TPrecision>)(Activator.CreateInstance(pBot.MaterializedType)
            ?? throw new Exception("Failed to create bot: " + pBot.MaterializedType));

        bot.Parameters = pBot;

        if (bot is IBarsBot<TPrecision> barsBot)
        {
            ArgumentNullException.ThrowIfNull(barsBot.Parameters.ExchangeSymbolTimeFrame.Symbol);

            barsBot.Parameters.ExchangeSymbolTimeFrame ??= barsBot.Parameters.ExchangeSymbolTimeFrame ?? throw new ArgumentNullException(nameof(barsBot.Parameters.ExchangeSymbolTimeFrame));

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

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Parents

    public abstract MultiSimContext MultiSimContext { get; }

    protected abstract ISimContext GetSimContext();

    #endregion

    #region Parameters

    public IEnumerable<PBotWrapper> PBacktests { get; }
    public BacktestExecutionOptions ExecutionOptions { get; } // MOVE to IBatchContext

    #region Derived

    // Must match across all parameters

    public TimeFrame TimeFrame => MultiSimContext.Parameters.DefaultTimeFrame ??  throw new ArgumentNullException(nameof(TimeFrame));
    public DateTimeOffset InputStart { get; }
    public DateTimeOffset Start => MultiSimContext.Parameters.Start;
    public DateTimeOffset EndExclusive => MultiSimContext.Parameters.EndExclusive;

    #endregion

    #endregion

    #region Lifecycle

    public BatchHarnessBase(IServiceProvider serviceProvider, PBatch pBatch)
    {
        ServiceProvider = serviceProvider;
        PBacktests = pBatch.PBacktests;

        var first = PBacktests.FirstOrDefault();
        if (first == null) throw new ArgumentException("batch empty");

        //DefaultTimeFrame = first.DefaultTimeFrame ?? (first.PBot as IPTimeFrameBot2)?.DefaultTimeFrame ?? throw new ArgumentNullException($"Neither first {nameof(parameters)} nor first {nameof(first.PBot)} has a {nameof(first.DefaultTimeFrame)}");
        //Start = first.Start;
        //EndExclusive = first.EndExclusive;

#if DEBUG
        // Extra validation to make sure stray backtests ended up in a sim with the wrong parameters

        //foreach (var p in PBacktests)
        //{
        //    if (p.DefaultTimeFrame != DefaultTimeFrame) throw new ArgumentException("DefaultTimeFrame mismatch");
        //    if (p.Start != Start) throw new ArgumentException("Start mismatch");
        //    if (p.EndExclusive != EndExclusive) throw new ArgumentException("EndExclusive mismatch");
        //}
#endif
    }
    
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
    
    public abstract ISimContext ISimContext { get; }

    #region Enumerators

    public Dictionary<string, InputEnumeratorBase> InputEnumerators { get; } = new();

    #endregion

    #endregion

    #region Init

    public IHistoricalTimeSeries Resolve(Type outputType, object source) => ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve(outputType, source);

    #endregion
}

