using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public class TDonchianChannel : TSingleSeriesIndicator
    {
        public TDonchianChannel()
        {
            base.Periods = 21;
        }
    }


    public class DonchianChannel : SingleSeriesIndicatorBase<TDonchianChannel>
    {

        #region Outputs

        public DataSeries Top { get; private set; } = new DataSeries();
        public DataSeries Middle { get; private set; } = new DataSeries();
        public DataSeries Bottom { get; private set; } = new DataSeries();

        public override IEnumerable<IndicatorDataSeries> Outputs
        {
            get
            {
                yield return Top;
                yield return Middle;
                yield return Bottom;
            }
        }
        
        #endregion

        #region Construction

        public DonchianChannel(TDonchianChannel config) : base(config)
        {
        }

        #endregion
        

        public  override Task CalculateIndex(int index)
        {
            
            double high = double.NaN;
            double low = double.NaN;
            var lookbackIndex = index - Template.Periods;

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
#if DEBUG
                    if (double.IsNaN(high) || double.IsNaN(low))
                    {
                        Debug.WriteLine("NaN detected in indicator at " + index);
                    }
#endif
                }
            }
            Top[index] = high;
            Bottom[index] = low;
            Middle[index] = (high + low) / 2.0;
            return Task.CompletedTask;
        }
    }
}
