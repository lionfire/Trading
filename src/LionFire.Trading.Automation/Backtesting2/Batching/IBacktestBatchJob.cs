namespace LionFire.Trading.Automation;

public interface IBacktestBatchJob
{
    Guid Guid { get; }

    /// <summary>
    /// Enumerable of all BacktestBatches, and an Enumerable of these in case the list is determined as it grows.
    /// </summary>
    IEnumerable<IEnumerable<IPBacktestTask2>> BacktestBatches { get; set; }
    IEnumerable<IPBacktestTask2> Backtests { set; }
    IPBacktestTask2 Backtest { set; }
   
    Task Task { get; }
    BacktestBatchJournal? Journal { get; set; }
}
