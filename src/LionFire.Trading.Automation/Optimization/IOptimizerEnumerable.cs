namespace LionFire.Trading.Automation.Optimization;

public interface IOptimizerEnumerable : IEnumerable<IEnumerable<IPBacktestTask2>>>
{
    IObservable<OptimizerProgress> Progress { get; }
}
