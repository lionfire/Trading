
namespace LionFire.Trading.Automation.Optimization.Enumerators;

public interface IOptimizerEnumerable : IEnumerable<IEnumerable<IPBacktestTask2>>
{
    IObservable<OptimizerProgress> Progress { get; }
}
