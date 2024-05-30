using LionFire.Trading.Automation;
using System.Collections;

namespace LionFire.Trading.Automation.Optimization.Enumerators;

public class ComprehensiveEnumerable : OptimizerEnumerableBase, IEnumerable<IEnumerable<IPBacktestTask2>>, IEnumerable<IPBacktestTask2>, IOptimizerEnumerable
{
    #region Lifecycle

    public ComprehensiveEnumerable(OptimizationTask optimizationTask)
    {
        OptimizationTask = optimizationTask;
    }

    public OptimizationTask OptimizationTask { get; }

    #endregion



    public IEnumerator<IPBacktestTask2> GetEnumerator()
    {
        // TODO SOON
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<IEnumerable<IPBacktestTask2>> IEnumerable<IEnumerable<IPBacktestTask2>>.GetEnumerator()
    {
        yield return this;
    }
}
