using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    // For compatibility with cAlgo, and for convenience
    public class EffectiveIndicators
    {
        IndicatorBase owner;
        public EffectiveIndicators(IndicatorBase owner)
        {
            this.owner = owner;
        }

        public DonchianChannel DonchianChannel(int periods)
        {
            return new DonchianChannel(new TDonchianChannel
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                Periods = periods,
            });
        }
    }
}
