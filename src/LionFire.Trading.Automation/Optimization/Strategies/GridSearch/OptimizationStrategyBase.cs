namespace LionFire.Trading.Automation.Optimization.Strategies;

public abstract class OptimizationStrategyBase
{
    public MultiBacktestContext BacktestContext => backtestContext;
    private MultiBacktestContext backtestContext;

    #region Parameters

    public POptimization OptimizationParameters { get; }

    #endregion

    protected OptimizationStrategyBase(MultiBacktestContext backtestContext, POptimization optimizationParameters)
    {
        this.backtestContext = backtestContext;
        OptimizationParameters = optimizationParameters ?? throw new ArgumentNullException();
    }


}
