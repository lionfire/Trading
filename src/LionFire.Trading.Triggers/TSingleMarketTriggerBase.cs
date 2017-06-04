using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Triggers
{
    public abstract class TSingleMarketTriggerBase<InstanceType> : TMarketTriggerBase<InstanceType>
        where InstanceType : class, new()
    {
        public override IEnumerable<string> Symbols { get { if (Symbol != null) { yield return Symbol; } } }
        public string Symbol { get; set; }
    }

}
