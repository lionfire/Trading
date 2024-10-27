using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;

namespace LionFire.Trading.Automation;

public class MultiBacktestContext
{
    #region Parameters

    public POptimization? OptimizationOptions { get; set; }
    
    // TODO: Populate this and hide elsewhere
    public BacktestExecutionOptions? ExecutionOptions { get; set; }

    public TradeJournalOptions? TradeJournalOptions { get; set; }

    #endregion

    #region State

    public int TradeJournalCount { get; set; }

    #region Derived

    public bool ShouldLogTradeDetails => (OptimizationOptions == null || TradeJournalCount < OptimizationOptions?.MaxDetailedJournals);

    #endregion

    #endregion

    #region (Public) Event Handlers

    public void OnTradeJournalCreated()
    {
        TradeJournalCount++;
        if (!ShouldLogTradeDetails) { TradeJournalOptions!.Enabled = false; }
    }

    #endregion

}

