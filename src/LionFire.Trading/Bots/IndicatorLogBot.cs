using System;
using System.Collections.Generic;
using System.Linq;
using LionFire.Trading.Indicators;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class IndicatorLogBot : MarketParticipant
    {
        DonchianChannel indicator;

        public IndicatorLogBot()
        {
            indicator = new DonchianChannel(new TDonchianChannel
            {
                Symbol =  "XAUUSD",
                TimeFrame = "h1",
                Periods = 35,
                Log = true,
                
            })
            {
                //Periods = 35,
            };

            DesiredSubscriptions = new List<MarketDataSubscription>()
                        {
                            //new MarketDataSubscription("XAUUSD", TimeFrame.m1),
                            new MarketDataSubscription("XAUUSD", TimeFrame.h1),
                            //new MarketDataSubscription("XAUUSD", TimeFrame.h2),
                            //new MarketDataSubscription("EURUSD", TimeFrame.m1),
                        };

        }

        long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            //if (indicator.MarketSeries == null)
            //{
            //    indicator.MarketSeries = Market.Data.LiveDataSources.GetMarketSeries("XAUUSD", TimeFrame.h1);
            //}

            indicator.Calculate(indicator.MarketSeries.Count - 1);

            if (i++ % 111 == 0)
            {
                Console.WriteLine($"{indicator.ToString()} h:{indicator.Top.LastValue} m:{indicator.Middle.LastValue} l:{indicator.Bottom.LastValue}");
                //Console.WriteLine($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            }
        }
    }
}