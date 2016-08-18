using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public class DonchianIndicator : IndicatorBase
    {
        public DonchianIndicator()
        {
            DesiredSubscriptions = new List<MarketDataSubscription>()
            {
                new MarketDataSubscription("XAUUSD", TimeFrame.m1),
                //new MarketDataSubscription("XAUUSD", TimeFrame.h2),
                //new MarketDataSubscription("EURUSD", TimeFrame.m1),
            };
        }

        long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            //if (i++ % 111 == 0)
            {
                Console.WriteLine($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            }
        }
    }
}
