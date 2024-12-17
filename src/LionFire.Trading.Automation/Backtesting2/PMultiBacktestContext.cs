using DynamicData;
using LionFire.Trading.Automation.Optimization;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.AccessControl;

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

    public long Completed => completedCount + (long)batchesInProgress.Items.Select(p => p.EffectiveCompleted).Sum();
    private long completedCount = 0;

    public IObservable<long> Completions => completions;
    private Subject<long> completions = new();
}

public class PMultiBacktestContext
{
    #region Lifecycle

    public PMultiBacktestContext(POptimization pOptimization)
    {
        POptimization = pOptimization;
    }

    public PMultiBacktestContext(Type pBotType, ExchangeSymbol? exchangeSymbol = null, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    {
        POptimization = new POptimization(pBotType, ExchangeSymbol)
        {
            CommonBacktestParameters = new()
            {
                Start = start ?? default,
                EndExclusive = endExclusive ?? default
            }
        };
    }

    public PMultiBacktestContext(IEnumerable<PBacktestTask2> pBacktestTask2, DateTimeOffset? start, DateTimeOffset? endExclusive)
    {
        var first = pBacktestTask2.First();
        var pBotType = first.PBot!.GetType();
        var exchangeSymbol = first.ExchangeSymbol ?? ExchangeSymbol.Unknown;

        POptimization = new POptimization(pBotType, exchangeSymbol)
        {
            CommonBacktestParameters = new()
            {
                Start = start ?? default,
                EndExclusive = endExclusive ?? default
            }
        };
    }

    #endregion

    public POptimization POptimization { get; set; }

    #region Derived

    #region Convenience

    public Type PBotType => POptimization.PBotType;
    public ExchangeSymbol ExchangeSymbol => POptimization.ExchangeSymbol;

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

    public MultiBacktestEvents Events { get; } = new();


}


