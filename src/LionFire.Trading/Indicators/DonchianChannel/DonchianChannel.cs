using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{

    public class TDonchianChannel : TIndicator
    {
        public int Periods { get; set; } = 21;
    }


    public class DonchianChannel : SingleSeriesIndicatorBase<TDonchianChannel>
    {

        #region Outpouts

        public DoubleDataSeries Top { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries Middle { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries Bottom { get; private set; } = new DoubleDataSeries();

        #endregion

        #region Construction

        public DonchianChannel(TDonchianChannel config) : base(config)
        {
        }

        #endregion

        #region Convenience

        public int Periods { get { return Template.Periods; } }

        #endregion

        protected override int CalculatedCount {
            get {
                return Top == null ? 0 : Top.Count;
            }
        }

        

        public override void Calculate(int upToIndex)
        {
            bool failedToGetHistoricalData = false;
            for (int index = Top.Count; index <= upToIndex; index++)
            {
                double high = double.NaN;
                double low = double.NaN;

                var lookbackIndex = index - Periods;

                if (lookbackIndex < MarketSeries.MinIndex)
                {
                    if (!failedToGetHistoricalData) { MarketSeries.EnsureDataAvailable(null, MarketSeries.OpenTime.First(), 2 + MarketSeries.MinIndex - lookbackIndex).Wait(); }
                    if (lookbackIndex < MarketSeries.MinIndex)
                    {
                        failedToGetHistoricalData = true; // REVIEW
                        // Market data not available
                        Top[index] = double.NaN;
                        Bottom[index] = double.NaN;
                        Middle[index] = double.NaN;
                        continue;
                    }
                }

                for (; lookbackIndex < index; lookbackIndex++)
                {
                    if (double.IsNaN(high))
                    {
                        high = MarketSeries.High[lookbackIndex];
                        low = MarketSeries.Low[lookbackIndex];
                    }
                    else
                    {
                        high = Math.Max(high, MarketSeries.High[index]);
                        low = Math.Min(low, MarketSeries.Low[index]);
                    }
                }
                Top[index] = high;
                Bottom[index] = low;
                Middle[index] = (high + low) / 2.0;
            }
        }

        long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            if (i++ % 111 == 0)
            {
                l.LogInformation($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            }
        }
    }
}
