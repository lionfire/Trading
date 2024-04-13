//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public sealed class TWilderSmoothingMovingAverage : TSingleSeriesIndicator
    {
        public TWilderSmoothingMovingAverage()
        {
            Periods = 14;
        }
    }

    /// <summary>
    ///  Same as EMA with the EMA period being 2x minus 1 as this period.
    /// </summary>
    public sealed class WilderSmoothingMovingAverage : SingleSeriesIndicatorBase<TWilderSmoothingMovingAverage>, IMovingAverageIndicator
    {
        public MovingAverageType Kind { get { return MovingAverageType.WilderSmoothing; } }

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

        public WilderSmoothingMovingAverage(TWilderSmoothingMovingAverage config) : base(config)
        {
            this.periodsD = this.periods = (Periods * 2) - 1;
        }

        #endregion

        private readonly int periods;
        private readonly double periodsD;

        public override Task CalculateIndex(int index) // Exact same algo as EMA.  Periods are different in constructor.
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
