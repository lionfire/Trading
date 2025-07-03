namespace LionFire.Trading.Automation.Optimization.Strategies;

public abstract class OptimizationStrategyBase
{

    public IBatchContext MultiBacktestContext => multiBacktestContext;
    private IBatchContext multiBacktestContext;

    public OptimizationTask OptimizationTask { get; }

    #region Parameters

    public POptimization OptimizationParameters { get; }

    #endregion

    protected OptimizationStrategyBase(OptimizationTask optimizationTask, POptimization optimizationParameters)
    {
        OptimizationTask = optimizationTask;
        OptimizationParameters = optimizationParameters ?? throw new ArgumentNullException();
    }

}
