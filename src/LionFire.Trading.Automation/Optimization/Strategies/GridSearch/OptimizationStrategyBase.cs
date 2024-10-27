namespace LionFire.Trading.Automation.Optimization.Strategies;

public abstract class OptimizationStrategyBase
{
    public MultiBacktestContext BacktestContext => backtestContext;
    private MultiBacktestContext backtestContext;

    protected OptimizationStrategyBase(MultiBacktestContext backtestContext)
    {
        this.backtestContext = backtestContext;
    }
}
