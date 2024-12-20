using Swordfish.NET.Collections;
using TEntry = (double fitness, System.Func<object, System.Threading.Tasks.ValueTask> discardAction, object paths);

namespace LionFire.Trading.Automation.Journaling.Trades;

public class BestJournalsTracker : IAsyncDisposable
{
    
    #region Parameters

    public int NumberToKeep { get; set; }

    #endregion

    #region Lifecycle

    public BestJournalsTracker(MultiBacktestContext context)
    {
        var options = context.POptimization.TradeJournalOptions;
        NumberToKeep = options.KeepTradeJournalsForTopNResults;
        Threshold = options.DiscardDetailsWhenFitnessBelow - double.Epsilon;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region State

    /// <summary>
    /// Volatile, but only goes up
    /// </summary>
    public double Threshold { get; set; }
    object lockObject = new();  
    public int NumberAboveThreshold { get; set; }

    #endregion

    SortedListWithDuplicates<TEntry> stats = new(new Comparer());
    class Comparer : IComparer<TEntry>
    {
        public int Compare(TEntry x, TEntry y) => x.fitness.CompareTo(y.fitness);
    }

    #region Methods

    public bool PeekShouldAdd(double fitness) => fitness > Threshold;

    public bool ShouldAdd(double fitness, Func<object, ValueTask> deleteAction, object paths)
    {
        if(fitness <= Threshold) { return false; }

        lock (lockObject)
        {
            if (fitness < Threshold) { return false; }

            stats.Add((fitness, deleteAction, paths));

            if (stats.Count > NumberToKeep)
            {
                Threshold = stats.First().fitness;
                var toDelete = stats[0];
                toDelete.discardAction(toDelete.paths);
                stats.RemoveAt(0);
            }
        }
        return true;
    }

    #endregion

}
