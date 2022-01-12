using LionFire.Applications;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting.Optimization
{
    public class TOptimizationTask : IHierarchicalTemplate, ITemplate<OptimizationTask>
    {
        //public List<ITemplate> Children { get; set; }
        InstantiationCollection Instantiations { get; set; }

        //IEnumerable<IInstantiation> IHierarchicalTemplate.Children => Children?.OfType<IInstantiation>(); // TODO: Cast/wrap to IInstantiation?  REVIEW the IHierarchicalTemplate interface.
        IInstantiationCollection IHierarchicalTemplate.Children => Instantiations;
    }

    public class OptimizationTask : AppTask
    {
        
    }
}
