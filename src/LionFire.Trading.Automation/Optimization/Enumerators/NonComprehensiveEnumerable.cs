using System.Collections;
using LionFire.Trading.Automation.Optimization.Enumerators;

namespace LionFire.Trading.Automation.Optimization;

public class NonComprehensiveEnumerable : OptimizerEnumerableBase
    , IEnumerable<IEnumerable<PBotWrapper>>
    , IEnumerable<PBotWrapper>
    , IOptimizerEnumerable
{

    #region Lifecycle

    public NonComprehensiveEnumerable(OptimizationTask optimizationTask)
    {
        OptimizationTask = optimizationTask;
    }

    public OptimizationTask OptimizationTask { get; }

    #endregion

    public IEnumerator<PBotWrapper> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<IEnumerable<PBotWrapper>> IEnumerable<IEnumerable<PBotWrapper>>.GetEnumerator()
    {
        yield return this;
    }
}
