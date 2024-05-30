using System.Reactive.Subjects;
using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation.Optimization.Enumerators;

public class OptimizerEnumerableBase
{
    public IObservable<OptimizerProgress> Progress => progress;
    protected BehaviorSubject<OptimizerProgress> progress = new(new());
}
