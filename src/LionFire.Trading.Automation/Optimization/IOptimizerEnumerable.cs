
namespace LionFire.Trading.Automation.Optimization.Enumerators;

public interface IOptimizerEnumerable : IEnumerable<IEnumerable<PBotWrapper>>
{
    IObservable<OptimizerProgress> Progress { get; }
}
