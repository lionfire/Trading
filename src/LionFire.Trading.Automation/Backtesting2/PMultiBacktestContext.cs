using DynamicData;
using LionFire.Applications.Trading;
using LionFire.ReactiveUI_;
using LionFire.Trading.Automation.Optimization;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.AccessControl;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation;

public class MultiBacktestEvents
{
    public void OnCompleted(long count)
    {
        completedCount += count;
        completions.OnNext(completedCount);
    }

    SourceCache<BacktestBatchProgress, int> batchesInProgress = new(b => b.BatchId);

    public void BatchStarting(BacktestBatchProgress progress)
        => batchesInProgress.AddOrUpdate(progress);

    public void BatchFinished(BacktestBatchProgress progress) => batchesInProgress.Remove(progress);

    public long FractionallyCompleted => completedCount + (long)batchesInProgress.Items.Select(p => p.EffectiveCompleted).Sum();
    public long Completed => completedCount;
    private long completedCount = 0;

    public IObservable<long> Completions => completions;
    private Subject<long> completions = new();
}

public class PMultiBacktestContext : DisposableBaseViewModel
{
    #region Lifecycle

    //public PMultiBacktestContext()
    //{
    //    POptimization = new(this);
    //}

    public static implicit operator PMultiBacktestContext(PMultiBacktest p) => new(p);

    public PMultiBacktestContext(PMultiBacktest? pMultiBacktest = null)
    {
        PMultiBacktest = pMultiBacktest ?? new();
        POptimization = new(this);
    }

    //[Obsolete]
    //public PMultiBacktestContext(Type pBotType, ExchangeSymbol? exchangeSymbol = null, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    //{
    //    if (start.HasValue)
    //    {
    //        PMultiBacktest.Start = start.Value;
    //    }
    //    if (endExclusive.HasValue)
    //    {
    //        PMultiBacktest.EndExclusive = endExclusive.Value;
    //    }

    //    PMultiBacktest.ExchangeSymbol = ExchangeSymbol;
    //    PMultiBacktest.PBotType = pBotType;
    //    POptimization = new POptimization(this);
    //}


    public PMultiBacktestContext(IEnumerable<PBacktestTask2> pBacktestTask2, DateTimeOffset? start, DateTimeOffset? endExclusive)
    {
        pBacktestTasks = pBacktestTask2;
        var first = pBacktestTask2.First();
        var pBotType = first.PBot!.GetType();
        var exchangeSymbol = first.ExchangeSymbol ?? ExchangeSymbol.Unknown;

        PMultiBacktest = new PMultiBacktest()
        {
            ExchangeSymbolTimeFrame = first.ExchangeSymbolTimeFrame,
            PBotType = first.PBot!.GetType(),
            Start = first.Start,
            EndExclusive = first.EndExclusive,
        };
        //if (start.HasValue)
        //{
        //    m.Start = start.Value;
        //}
        //if (endExclusive.HasValue)
        //{
        //    m.EndExclusive = endExclusive.Value;
        //}

        //m.PBotType = pBotType;
        
        POptimization = new POptimization(this);
    }

    #endregion

    #region State


    public PMultiBacktest PMultiBacktest
    {
        get => pMultiBacktest;
        set => RaiseAndSetNestedViewModelIfChanged(ref pMultiBacktest, value);
    }
    private PMultiBacktest pMultiBacktest = new();

    #endregion

    #region Backtests

    public IEnumerable<PBacktestTask2> PBacktests => pBacktestTasks;
    IEnumerable<PBacktestTask2>? pBacktestTasks;


    #endregion

    

    public POptimization POptimization
    {
        get => pOptimization;
        set
        {
            if (value.Parent != this)
            {
                throw new Exception();
                //value.Parent = this;
            }
            RaiseAndSetNestedViewModelIfChanged(ref pOptimization, value);
        }
    }
    POptimization pOptimization;

    #region Derived

    //public TimeFrame? TimeFrame => pBacktestTasks?.FirstOrDefault()?.TimeFrame;
    //public DateTimeOffset? Start => pBacktestTasks?.FirstOrDefault()?.Start;
    //public DateTimeOffset? EndExclusive => pBacktestTasks?.FirstOrDefault()?.EndExclusive;

    #region Convenience

    public Type? PBotType => PMultiBacktest.PBotType;
    public ExchangeSymbol ExchangeSymbol => PMultiBacktest.ExchangeSymbolTimeFrame!;  // REVIEW nullability

    #endregion

    public Type? BotType => botType ??= PBotType == null ? null : BotTyping.TryGetBotType(PBotType);
    private Type? botType;

    #endregion

}

public static class BotTyping
{

    public static Type TryGetBotType( Type pBotType)
    {
        Type? botType;
        if (pBotType.IsAssignableTo(typeof(IPBot2Static)))
        {
            botType = (Type)pBotType.GetProperty(nameof(IPBot2Static.StaticMaterializedType))!.GetValue(null)!;
        }
        else
        {
            throw new ArgumentException($"Provide {nameof(botType)} or a {nameof(pBotType)} of type IPBot2Static");
        }

        return botType;
    }

}

