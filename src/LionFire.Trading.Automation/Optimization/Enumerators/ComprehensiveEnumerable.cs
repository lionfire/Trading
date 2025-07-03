using LionFire.Trading.Automation;
using System.Collections;

namespace LionFire.Trading.Automation.Optimization.Enumerators;

public class ComprehensiveEnumerable : OptimizerEnumerableBase, IEnumerable<IEnumerable<PBotWrapper>>, IEnumerable<PBotWrapper>, IOptimizerEnumerable
{
    #region Lifecycle

    public ComprehensiveEnumerable(OptimizationTask optimizationTask)
    {
        OptimizationTask = optimizationTask;
    }

    public OptimizationTask OptimizationTask { get; }

    #endregion



    public IEnumerator<PBotWrapper> GetEnumerator()
    {
        // TODO SOON
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<IEnumerable<PBotWrapper>> IEnumerable<IEnumerable<PBotWrapper>>.GetEnumerator()
    {
        yield return this;
    }
}
