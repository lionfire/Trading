using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{


    public class TBollingerBands : TSingleSeriesIndicator, ITSingleSeriesIndicator
    {
        public int Periods { get; set; } = 20;
        public double StandardDev { get; set; } = 2.0;
        public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;
    }


    public class BollingerBands : SingleSeriesIndicatorBase<TBollingerBands>
    {

        #region Outputs

        public DoubleDataSeries Top { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries Middle { get; private set; } = new DoubleDataSeries();
        public DoubleDataSeries Bottom { get; private set; } = new DoubleDataSeries();

        public override IEnumerable<IDataSeries> Outputs
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

        public BollingerBands(TBollingerBands config) : base(config)
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            movingAverage = EffectiveIndicators.MovingAverage(Template.MovingAverageType, Template.Periods);
        }

        #endregion

        IMovingAverageIndicator movingAverage;

        private IEnumerable<double> LastPeriodValues
        {
            get
            {
                var periods = Template.Periods;
                for (int i = 0; i < periods; i++)
                {
                    yield return DataSeries.Last(i);
                }
            }
        }

        protected override void CalculateIndex(int index)
        {
            /*
             *  Middle Band = 20-day simple moving average (SMA)
             * Upper Band = 20-day SMA + (20-day standard deviation of price x 2) 
             * Lower Band = 20-day SMA - (20-day standard deviation of price x 2)
             * 
             */
            movingAverage.Calculate(index);
            var ma = movingAverage.Result[index];
            Middle[index] = ma;

            // TODO VERIFY - what happens if NaN's exist in DataSeries?

            var stdDevDoubled = Template.StandardDev * LastPeriodValues.StdDev();
            Top[index] = ma;
            Bottom[index] = ma;
        }
    }

    public static class Extensions
    {
        public static double StdDev(this IEnumerable<double> values)
        {
            double ret = 0;
            int count = values.Count();
            if (count > 1)
            {
                double avg = values.Average();                
                double sum = values.Sum(value => (value - avg) * (value - avg));

                ret = Math.Sqrt(sum / count);
            }
            return ret;
        }
    }
}

