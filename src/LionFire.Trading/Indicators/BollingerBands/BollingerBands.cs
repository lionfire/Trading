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
        public TBollingerBands()
        {
            Periods = 20;
        }


        public double StandardDev { get; set; } = 2.0;
        public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;
    }


    public partial class EffectiveIndicators
    {
        public BollingerBands BollingerBands(DataSeries series, int periods, double standardDeviation, MovingAverageType movingAverageType)
        {

            return new BollingerBands(new TBollingerBands
            {
                TimeFrame = owner.TimeFrame,
                Symbol = owner.Symbol.Code,
                Periods = periods,
                StandardDev = standardDeviation,
                MovingAverageType = movingAverageType,
                IndicatorBarSource = series,
            });
        }
    }

    public class BollingerBands : SingleSeriesIndicatorBase<TBollingerBands>
    {

        #region Outputs

        public DataSeries Top { get; private set; } = new DataSeries();
        public DataSeries Main { get; private set; } = new DataSeries();
        public DataSeries Bottom { get; private set; } = new DataSeries();

        public override IEnumerable<IndicatorDataSeries> Outputs
        {
            get
            {
                yield return Top;
                yield return Main;
                yield return Bottom;
            }
        }

        #endregion

        public override IEnumerable<IAccountParticipant> Children
        {
            get
            {
                yield return movingAverage;
            }
        }

        #region Construction

        public BollingerBands(TBollingerBands config) : base(config)
        {
        }
        protected override void OnInitializing()
        {            
            movingAverage = EffectiveIndicators.MovingAverage(Template.MovingAverageType, Template.Periods); // Must be before base.Initializing
            base.OnInitializing();
        }
        protected override void OnInitialized()
        {
            
            base.OnInitialized();
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

        public override Task CalculateIndex(int index)
        {
            
            /*
             *  Middle Band = 20-day simple moving average (SMA)
             * Upper Band = 20-day SMA + (20-day standard deviation of price x 2) 
             * Lower Band = 20-day SMA - (20-day standard deviation of price x 2)
             * 
             */
            movingAverage.CalculateIndex(index);
            var ma = movingAverage.Result[index];
            Main[index] = ma;

            // TODO VERIFY - what happens if NaN's exist in DataSeries?

            var stdDevDoubled = Template.StandardDev * LastPeriodValues.StdDev();
            Top[index] = ma;
            Bottom[index] = ma;
            return Task.CompletedTask;
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

