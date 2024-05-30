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
        public IEnumerable<IEnumerable<IPBacktestTask2>> BacktestBatches { get; set; } = [];

        #region Derived (convenience)

        public IEnumerable<IPBacktestTask2> Backtests { set => BacktestBatches = [value]; }
        public IPBacktestTask2 Backtest { set => BacktestBatches = [[value]]; }

        #endregion

        #endregion

        #region Lifecycle

        public BacktestBatchesJob(Guid guid)
        {
            Guid = guid;
        }

        #endregion

        #region State

        public Task Task => tcs.Task;

        TaskCompletionSource tcs = new();
        internal void OnFinished() => tcs.SetResult();

        #endregion
    }
}
