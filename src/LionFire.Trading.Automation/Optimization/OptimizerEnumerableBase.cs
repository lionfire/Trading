using System.Reactive.Subjects;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizerEnumerableBase
{
    public IObservable<OptimizerProgress> Progress => progress;
    protected BehaviorSubject<OptimizerProgress> progress = new(new());
}
