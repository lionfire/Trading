using LionFire.Structures;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Structures;
using Newtonsoft.Json;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizationRunStatsGeneratorConfig
{
    public double HistogramBucketStep = 0.5;
    public double HistogramMin = -3.5;
    public double HistogramMax = 5.5;

}

public class OptimizationRunStatsGenerator
{

    public static OptimizationRunStats Generate(OptimizationRunBacktests optimizationRunBacktests)
    {
        var backtests = optimizationRunBacktests.Backtests;
        var stats = new OptimizationRunStats();
        stats.BacktestsCount = backtests.Count;
        stats.GeneratedOn = DateTime.UtcNow;
        stats.Version = OptimizationRunStats.CurrentVersion;

        stats.AD = backtests.Select(b => b.AD).GetStats(nameof(BacktestResult.AD));
        stats.Fitness = backtests.Select(b => b.Fitness).GetStats(nameof(BacktestResult.Fitness));
        stats.Aroi = backtests.Select(b => b.Aroi).GetStats(nameof(BacktestResult.Aroi));


        stats.AverageDaysPerTrade = backtests.Select(b => b.AverageDaysPerTrade).GetStats(nameof(BacktestResult.AverageDaysPerTrade));
        stats.AverageDaysPerWinningTrade = backtests.Select(b => b.AverageDaysPerWinningTrade).GetStats(nameof(BacktestResult.AverageDaysPerWinningTrade));
        stats.AverageDaysPerLosingTrade = backtests.Select(b => b.AverageDaysPerLosingTrade).GetStats(nameof(BacktestResult.AverageDaysPerLosingTrade));

        stats.TradesPerMonth = backtests.Select(b => b.TradesPerMonth).GetStats(nameof(BacktestResult.TradesPerMonth));

        stats.TotalTrades = backtests.Select(b => b.TotalTrades).GetStats(nameof(BacktestResult.TotalTrades));
        stats.WinRate = backtests.Select(b => b.WinRate).GetStats(nameof(BacktestResult.WinRate));
        // Missing:
        //LosingTrades
        //WinningTrades

        stats.AverageTrade = backtests.Select(b => b.AverageTrade).GetStats(nameof(BacktestResult.AverageTrade));
        stats.AverageTradePerVolume = backtests.Select(b => b.AverageTradePerVolume).GetStats(nameof(BacktestResult.AverageTradePerVolume));

        stats.ProfitFactor = backtests.Select(b => b.ProfitFactor).GetStats(nameof(BacktestResult.ProfitFactor));
        stats.SharpeRatio = backtests.Select(b => b.SharpeRatio).GetStats(nameof(BacktestResult.SharpeRatio));
        stats.SortinoRatio = backtests.Select(b => b.SortinoRatio).GetStats(nameof(BacktestResult.SortinoRatio));

        // Missing:
        //MaxBalanceDrawdown
        //MaxBalanceDrawdownPercentages
        //MaxEquityDrawdown
        //MaxEquityDrawdownPercentages

        var c = ManualSingleton<OptimizationRunStatsGeneratorConfig>.GuaranteedInstance;

        Histogram h(IEnumerable<double> data) => new Histogram(c.HistogramBucketStep, c.HistogramMin, c.HistogramMax, data);


        // ENH: AverageTradeByADHistogram, etc.
        stats.ADHistogram = h(backtests.Select(b => b.AD));
        stats.DADHistogram = h(backtests.Select(b => b.DAD));
        stats.NADHistogram = h(backtests.Select(b => b.NAD));
        stats.PADHistogram = h(backtests.Select(b => b.PAD));
        stats.AADHistogram = h(backtests.Select(b => b.AAD));

        return stats;
    }

}
