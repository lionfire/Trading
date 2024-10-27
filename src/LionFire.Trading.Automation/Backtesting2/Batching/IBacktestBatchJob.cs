using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation;

public interface IBacktestBatchJob
{
    Guid Guid { get; }

    /// <summary>
    /// Enumerable of all BacktestBatches, and an Enumerable of these in case the list is determined as it grows.
    /// </summary>
    IEnumerable<IEnumerable<PBacktestTask2>> BacktestBatches { get; set; }
    IEnumerable<PBacktestTask2> Backtests { set; }
    PBacktestTask2 Backtest { set; }
   
    Task Task { get; }
    BacktestBatchJournal? Journal { get; set; }
    MultiBacktestContext Context { get;  }
}
