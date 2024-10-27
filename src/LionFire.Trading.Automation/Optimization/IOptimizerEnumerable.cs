
namespace LionFire.Trading.Automation.Optimization.Enumerators;

public interface IOptimizerEnumerable : IEnumerable<IEnumerable<PBacktestTask2>>
{
    IObservable<OptimizerProgress> Progress { get; }
}
