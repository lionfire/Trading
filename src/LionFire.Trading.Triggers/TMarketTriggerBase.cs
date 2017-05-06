using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Triggers
{
    public abstract class TMarketTriggerBase
    {
        public abstract IEnumerable<string> Symbols { get; }
    }
    public abstract class TSingleMarketTriggerBase : TMarketTriggerBase
    {
        public override IEnumerable<string> Symbols { get { if (Symbol != null) { yield return Symbol; } } }
        public string Symbol { get; set; }
    }
}
