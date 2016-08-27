using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{


    public class DonchianIndicator : SingleSymbolIndicatorBase
    {

        public int Periods { get; set; } = 21;

        public DoubleDataSeries High { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries Mid { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries Low { get; private set; } = new DoubleDataSeries();

        public DonchianIndicator()
        {
            //DesiredSubscriptions = new List<MarketDataSubscription>()
            //{
            //    new MarketDataSubscription("XAUUSD", TimeFrame.m1),
            //    //new MarketDataSubscription("XAUUSD", TimeFrame.h2),
            //    //new MarketDataSubscription("EURUSD", TimeFrame.m1),
            //};
        }

        public override void Calculate(int index)
        {
            if (High.Count == 0)
            {
                High.Add(0);
                Mid.Add(0);
                Low.Add(0);
            }
            else
            {
                double high = double.NaN;
                double low = double.NaN;
                for (int i = index - Periods; i < index; i++)
                {
                    if (i < 0)
                    {
                        High.Add(double.NaN);
                        Low.Add(double.NaN);
                        Mid.Add(double.NaN);
                        break;
                    }

                    if (double.IsNaN(high))
                    {
                        high = MarketSeries.High[index];
                        low = MarketSeries.Low[index];
                    }
                    else
                    {
                        high = Math.Max(high, MarketSeries.High[index]);
                        low = Math.Min(low, MarketSeries.Low[index]);
                    }
                }

                High.Add(high);
                Low.Add(low);
                Mid.Add((high + low) / 2.0);
            }
        }


        //long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            //if (i++ % 111 == 0)
            {
                Console.WriteLine($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            }
        }
    }
}
