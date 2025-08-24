using LionFire.Trading.Optimization;

namespace LionFire.Trading.Automation.Optimization.Strategies;

public interface IOptimizationStrategy
{
    long BacktestsComplete { get; }

    long MinBacktestsRemaining { get; }
    long MaxBacktestsRemaining { get; }
    OptimizationProgress Progress { get; }
}
