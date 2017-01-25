//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public sealed class TExponentialMovingAverage : TSingleSeriesIndicator
    {
        public TExponentialMovingAverage()
        {
            Periods = 14;
        }
    }

    // Reference: http://www.iexplain.org/ema-how-to-calculate/
    public sealed class ExponentialMovingAverage : SingleSeriesIndicatorBase<TExponentialMovingAverage>, IMovingAverageIndicator
    {
public MovingAverageType Kind { get{return MovingAverageType.Exponential; }}

        #region Outputs

        public DataSeries Result { get; private set; } = new DataSeries();

        public override IEnumerable<IndicatorDataSeries> Outputs
        {
            get
            {
                yield return Result;
            }
        }

        #endregion

        #region Lifecycle

        public ExponentialMovingAverage(TExponentialMovingAverage config) : base(config)
        {
            periodsD = periods = Periods;

        }

        #endregion

        readonly int periods;
        readonly double periodsD;

        public override Task CalculateIndex(int index)
        {
            double avg = Result[index - 1];

            if (!double.IsNaN(avg))
            {
                var multiplier = (2 / (Periods + 1));
                // simplified version of: todaysPrice * multiplier + EMAYesterday * (1 – multiplier);
                avg = (DataSeries[index] - avg) * multiplier + avg;
            }
            else // Sum
            {
                avg = 0;
                {
                    for (int i = periods; i > 0; i--)
                    {
                        avg += DataSeries.Last(i);
                    }
                    avg /= periodsD;
                }
            }
            Result[index] = avg;
            return Task.CompletedTask;

        }
    }
}
