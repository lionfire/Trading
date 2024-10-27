using System.Collections;
using LionFire.Trading.Automation.Optimization.Enumerators;

namespace LionFire.Trading.Automation.Optimization;

public class NonComprehensiveEnumerable : OptimizerEnumerableBase
    , IEnumerable<IEnumerable<PBacktestTask2>>
    , IEnumerable<PBacktestTask2>
    , IOptimizerEnumerable
{

    #region Lifecycle

    public NonComprehensiveEnumerable(OptimizationTask optimizationTask)
    {
        OptimizationTask = optimizationTask;
    }

    public OptimizationTask OptimizationTask { get; }

    #endregion

    public IEnumerator<PBacktestTask2> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<IEnumerable<PBacktestTask2>> IEnumerable<IEnumerable<PBacktestTask2>>.GetEnumerator()
    {
        yield return this;
    }
}
