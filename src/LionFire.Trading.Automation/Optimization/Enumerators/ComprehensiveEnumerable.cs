using LionFire.Trading.Automation;
using System.Collections;

namespace LionFire.Trading.Automation.Optimization.Enumerators;

public class ComprehensiveEnumerable : OptimizerEnumerableBase, IEnumerable<IEnumerable<PBacktestTask2>>, IEnumerable<PBacktestTask2>, IOptimizerEnumerable
{
    #region Lifecycle

    public ComprehensiveEnumerable(OptimizationTask optimizationTask)
    {
        OptimizationTask = optimizationTask;
    }

    public OptimizationTask OptimizationTask { get; }

    #endregion



    public IEnumerator<PBacktestTask2> GetEnumerator()
    {
        // TODO SOON
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<IEnumerable<PBacktestTask2>> IEnumerable<IEnumerable<PBacktestTask2>>.GetEnumerator()
    {
        yield return this;
    }
}
