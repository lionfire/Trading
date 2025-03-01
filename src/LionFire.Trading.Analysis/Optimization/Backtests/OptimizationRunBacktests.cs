using LionFire.Trading.Backtesting;
using System.Collections.Generic;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizationRunBacktests
{
    public List<BacktestResult> Backtests { get; set; } = new();
    public OptimizationParameters Parameters { get; set; } = new();
}

