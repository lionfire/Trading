using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, IHasSingleSeries
        where TConfig : IIndicatorConfig
    {
        public MarketSeries MarketSeries { get; protected set; }
    }

}
