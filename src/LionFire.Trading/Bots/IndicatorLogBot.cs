using System;
using System.Collections.Generic;
using System.Linq;
using LionFire.Trading.Indicators;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class IndicatorLogBot : MarketParticipant
    {
        DonchianIndicator indicator;

        public IndicatorLogBot()
        {
            indicator = new DonchianIndicator()
            {
                Periods = 35,
            };
/*
            DesiredSubscriptions = new List<MarketDataSubscription>()
            {
                //new MarketDataSubscription("XAUUSD", TimeFrame.m1),
                new MarketDataSubscription("XAUUSD", TimeFrame.h1),
                //new MarketDataSubscription("XAUUSD", TimeFrame.h2),
                //new MarketDataSubscription("EURUSD", TimeFrame.m1),
            };
*/
        }

        //IObservable<SymbolBar> Bars { get { return bars;  } }
        //System.Reactive.Subjects.SubjectBase
        //Subject<SymbolBar> bars = new Subject<SymbolBar>();
        //long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            if (indicator.MarketSeries == null)
            {
                indicator.MarketSeries = Market.Data.LiveDataSources.GetMarketSeries("XAUUSD", TimeFrame.h1);
            }

            indicator.Calculate(indicator.MarketSeries.Count - 1);

            //if (i++ % 111 == 0)
            {
                Console.WriteLine($"{indicator.Name} h:{indicator.High.LastValue} m:{indicator.Mid.LastValue} l:{indicator.Low.LastValue}");
                //Console.WriteLine($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            }
        }
    }
}