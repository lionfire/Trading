using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    

    public abstract class SingleSymbolIndicatorBase : IndicatorBase<IndicatorConfig>
    {
        
        public IMarketSeries MarketSeries { get; set; }

        public string Name {
            get {
                if (name == null)
                {
                    name = this.GetType().Name.Replace("Indicator", "");
                }
                return name;
            }

        }
        private string name;
    }
}
