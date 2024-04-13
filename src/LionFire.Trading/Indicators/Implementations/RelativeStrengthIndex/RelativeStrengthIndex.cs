//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public class TRelativeStrengthIndex : TSingleSeriesIndicator
    {
        public TRelativeStrengthIndex()
        {
            Periods = 14;
        }

        //public double OverBoughtLevel { get; set; } = 70;
        //public double OverSoldLevel { get; set; } = 30;

    }


    public class RelativeStrengthIndex : SingleSeriesIndicatorBase<TRelativeStrengthIndex>
    {

        #region Outputs

        public DataSeries Result { get; private set; } = new DataSeries();

#if OptimizeSpeed
        public DoubleDataSeries AvgGain { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries AvgLoss { get; private set; } = new DoubleDataSeries();
#endif

        public override IEnumerable<IndicatorDataSeries> Outputs
        {
            get
            {
                yield return Result;
            }
        }

        #endregion


        #region Lifecycle

        public RelativeStrengthIndex(TRelativeStrengthIndex config) : base(config)
        {
        }

        #endregion


        public override Task CalculateIndex(int index)
        {
            /*
                                       100
                RSI = 100 - --------
                                      1 + RS

                RS = Average Gain / Average Loss

            First Average Gain = Sum of Gains over the past 14 periods / 14.
            First Average Loss = Sum of Losses over the past 14 periods / 14

            Average Gain = [(previous Average Gain) x 13 + current Gain] / 14.
            Average Loss = [(previous Average Loss) x 13 + current Loss] / 14.

            */

            var periods = Template.Periods;

            double gainAvg;
            double lossAvg;
#if OptimizeSpeed
             gainAvg = AvgGain[index - 1];
             lossAvg = AvgLoss[index - 1];

            if (!double.IsNaN(gainAvg))
            {
                var curGain = Math.Max(0, DataSeries.Last(i) - DataSeries.Last(i + 1));
                var curLoss = Math.Max(0, DataSeries.Last(i + 1) - DataSeries.Last(i));

                gainAvg = ((gainAvg * periods - 1) + curGain) / periods;
                lossAvg = ((lossAvg * periods - 1) + curLoss) / periods;



            }
            else
#else
            gainAvg = 0;
            lossAvg = 0;
#endif
            {
                gainAvg = 0;
                lossAvg = 0;
                for (int i = periods; i > 0; i--)
                {
                    gainAvg += Math.Max(0, DataSeries.Last(i) - DataSeries.Last(i + 1));
                    lossAvg += Math.Max(0, DataSeries.Last(i + 1) - DataSeries.Last(i));
                }
                gainAvg /= periods;
                lossAvg /= periods;
            }

#if OptimizeSpeed
            AvgGain[index] = gainAvg;
            AvgLoss[index] = lossAvg;
#endif

            Result[index] = 100 - (100 / (1.0 + (gainAvg / lossAvg)));

            //   var avgGain = 


            //            double high = double.NaN;
            //            double low = double.NaN;
            //            var lookbackIndex = index - Periods;

            //            for (; lookbackIndex < index; lookbackIndex++)
            //            {
            //                if (double.IsNaN(high))
            //                {
            //                    high = MarketSeries.High[lookbackIndex];
            //                    low = MarketSeries.Low[lookbackIndex];
            //                }
            //                else
            //                {
            //                    high = Math.Max(high, MarketSeries.High[index]);
            //                    low = Math.Min(low, MarketSeries.Low[index]);
            //#if DEBUG
            //                    if (double.IsNaN(high) || double.IsNaN(low))
            //                    {
            //                        Debug.WriteLine("NaN detected in indicator at " + index);
            //                    }
            //#endif
            //                }
            //            }
            //            Top[index] = high;
            //            Bottom[index] = low;
            //            Middle[index] = (high + low) / 2.0;
            return Task.CompletedTask;
        }


    }
}
