using System.Collections;
using LionFire.Trading.Automation.Optimization.Enumerators;

namespace LionFire.Trading.Automation.Optimization;

public class NonComprehensiveEnumerable : OptimizerEnumerableBase
    , IEnumerable<IEnumerable<IPBacktestTask2>>
    , IEnumerable<IPBacktestTask2>
    , IOptimizerEnumerable
{

    #region Lifecycle

    public NonComprehensiveEnumerable(OptimizationTask optimizationTask)
    {
        OptimizationTask = optimizationTask;
    }

    public OptimizationTask OptimizationTask { get; }

    #endregion

    public IEnumerator<IPBacktestTask2> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<IEnumerable<IPBacktestTask2>> IEnumerable<IEnumerable<IPBacktestTask2>>.GetEnumerator()
    {
        yield return this;
    }
}
