using System.Collections.Generic;

namespace LionFire.Trading.Triggers
{
    public abstract class TMultiMarketTriggerBase<InstanceType> : TMarketTriggerBase<InstanceType>
        where InstanceType : class, new()
    {
        public override IEnumerable<string> Symbols => SymbolList;
        public List<string> SymbolList { get; set; }
    }
}
