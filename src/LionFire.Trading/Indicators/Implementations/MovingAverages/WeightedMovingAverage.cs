//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public class TWeightedMovingAverage : TSingleSeriesIndicator
    {
        public TWeightedMovingAverage()
        {
            Periods = 14;
        }
    }

    public class WeightedMovingAverage : SingleSeriesIndicatorBase<TWeightedMovingAverage>, IMovingAverageIndicator
    {
        public MovingAverageType Kind { get { return MovingAverageType.Weighted; } }

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

        public WeightedMovingAverage(TWeightedMovingAverage config) : base(config)
        {
            sum = 0;
            for (int i = Periods; i > 0; i--)
            {
                sum += i;
            }
            periods = this.Periods;
        }

        #endregion

        private double sum;
        private int periods;

        public override Task CalculateIndex(int index)
        {
            double avg = 0;

            for (int i = 0; i < periods; i++)
            {
                var data = DataSeries[index - i];
                if (double.IsNaN(data)) { Result[index] = double.NaN; return Task.CompletedTask; }

                avg += data * (periods - i);
            }
            Result[index] = avg / sum;
            // TOVERIFY
            return Task.CompletedTask;
        }
    }
}
