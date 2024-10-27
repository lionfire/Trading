using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation;


public partial class BacktestQueue
{
    /// <summary>
    /// A batch of backtests.
    /// Must all have the same start and end date.
    /// </summary>
    private class BacktestBatchesJob : IBacktestBatchJob
    {
        #region Identity

        public Guid Guid { get; }

        #endregion

        #region Backtest parameters

        /// <summary>
        /// An enumerable of enumerables of backtests.
        /// After each round of backtests, the producer may decide on the next round, such as in the case of 
        /// a non-comprehensive optimization that will delve deeper into paths that seem most promising.
        /// </summary>
        public IEnumerable<IEnumerable<PBacktestTask2>> BacktestBatches { get; set; } = [];
        public MultiBacktestContext Context { get; }

        public BacktestBatchJournal? Journal { get; set; }

        #region Derived (convenience)

        public IEnumerable<PBacktestTask2> Backtests { set => BacktestBatches = [value]; }
        public PBacktestTask2 Backtest { set => BacktestBatches = [[value]]; }

        public int Count => BacktestBatches.Aggregate(0, (acc, batch) => acc + batch.Count());

        #endregion

        #endregion

        #region Lifecycle

        public BacktestBatchesJob(Guid guid, MultiBacktestContext context)
        {
            Guid = guid;
            Context = context;
        }

        #endregion

        #region State

        public Task Task => tcs.Task;


        TaskCompletionSource tcs = new();
        internal void OnFinished() => tcs.SetResult();
        internal void OnFaulted(Exception exception) => tcs.SetException(exception);

        #endregion
    }
}
