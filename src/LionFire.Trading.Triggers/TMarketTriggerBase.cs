using LionFire.Instantiating;
using LionFire.Triggers;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Triggers
{

    public abstract class TMarketTriggerBase<InstanceType> : TTriggerBase, ITemplate<InstanceType>
        where InstanceType : class, new()
    {
        public abstract IEnumerable<string> Symbols { get; }
    }
}
