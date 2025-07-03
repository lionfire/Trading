using DynamicData;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LionFire.Trading.Automation;

public class MultiBatchEvents
{
    public void OnCompleted(long count)
    {
        completedCount += count;
        completions.OnNext(completedCount);
    }

    SourceCache<BacktestBatchProgress, int> batchesInProgress = new(b => b.BatchId);

    public void BatchStarting(BacktestBatchProgress progress)
        => batchesInProgress.AddOrUpdate(progress);

    public void BatchFinished(BacktestBatchProgress? progress)
    {
        if (progress != null)
        {
            batchesInProgress.Remove(progress);
        }
    }

    public long FractionallyCompleted => completedCount + (long)batchesInProgress.Items.Select(p => p.EffectiveCompleted).Sum();
    public long Completed => completedCount;
    private long completedCount = 0;

    public IObservable<long> Completions => completions;
    private Subject<long> completions = new();
}

