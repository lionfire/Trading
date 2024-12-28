using DynamicData;
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

    public PMultiBacktestContext()
    {
        POptimization = new(this);
    }

    public PMultiBacktestContext(Type pBotType, ExchangeSymbol? exchangeSymbol = null, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    {
        if (start.HasValue)
        {
            CommonBacktestParameters.Start = start.Value;
        }
        if (endExclusive.HasValue)
        {
            CommonBacktestParameters.EndExclusive = endExclusive.Value;
        }

        CommonBacktestParameters.ExchangeSymbol = ExchangeSymbol;
        CommonBacktestParameters.PBotType = pBotType;
        POptimization = new POptimization(this);
    }

    public PMultiBacktestContext(IEnumerable<PBacktestTask2> pBacktestTask2, DateTimeOffset? start, DateTimeOffset? endExclusive)
    {
        var first = pBacktestTask2.First();
        var pBotType = first.PBot!.GetType();
        var exchangeSymbol = first.ExchangeSymbol ?? ExchangeSymbol.Unknown;

        if (start.HasValue)
        {
            CommonBacktestParameters.Start = start.Value;
        }
        if (endExclusive.HasValue)
        {
            CommonBacktestParameters.EndExclusive = endExclusive.Value;
        }

        CommonBacktestParameters.ExchangeSymbol = ExchangeSymbol;
        CommonBacktestParameters.PBotType = pBotType;
        POptimization = new POptimization(this);
    }

    #endregion

    public PBacktestBatchTask2 CommonBacktestParameters
    {
        get => commonBacktestParameters;
        set => RaiseAndSetNestedViewModelIfChanged(ref commonBacktestParameters, value);
    }
    private PBacktestBatchTask2 commonBacktestParameters = new();

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

    #region Convenience

    public Type PBotType => POptimization.PBotType;
    public ExchangeSymbol ExchangeSymbol => CommonBacktestParameters.ExchangeSymbol;

    #endregion

    public Type BotType => botType ??= TryGetBotType(PBotType);
    private Type? botType;

    private static Type TryGetBotType(Type pBotType)
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

    #endregion



}


