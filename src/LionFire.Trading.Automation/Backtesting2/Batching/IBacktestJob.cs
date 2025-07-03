using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation;

#if OLD // Not needed. Might be needed if TPrecision is ever used in BacktestJob.
public interface IBacktestJob
{
    #region Identity

    Guid Guid { get; }

    #endregion

    #region Parameters

    /// <summary>
    /// Enumerable of all BacktestBatches, and an Enumerable of these in case the list is determined as it grows.
    /// </summary>
    IEnumerable<IEnumerable<PBotWrapper>> BacktestBatches { get; set; }
    IEnumerable<PBotWrapper> Backtests { set; }
    PBotWrapper Backtest { set; }

    #endregion

    #region State

    MultiSimContext MultiSimContext { get; }
    Task Task { get; }

    //BacktestsJournal? JobJournal => MultiSimContext.Journal;

    #endregion

}
#endif