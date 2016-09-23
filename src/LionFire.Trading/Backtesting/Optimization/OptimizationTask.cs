using LionFire.Applications;
using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting.Optimization
{
    public class TOptimizationTask : IHierarchicalTemplate, ITemplate<OptimizationTask>
    {
        public List<ITemplate> Children { get; set; }
    }

    public class OptimizationTask : AppTask
    {
        
        protected override void Run()
        {
            base.Run();
        }
    }
}
