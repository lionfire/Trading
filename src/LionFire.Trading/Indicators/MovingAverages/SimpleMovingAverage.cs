//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public sealed class TSimpleMovingAverage : TSingleSeriesIndicator
    {
        public TSimpleMovingAverage()
        {
            Periods = 14;
        }
    }
    
    public sealed class SimpleMovingAverage : SingleSeriesIndicatorBase<TSimpleMovingAverage>, IMovingAverageIndicator
    {

        public MovingAverageType Kind { get { return MovingAverageType.Simple; } }

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

        public SimpleMovingAverage(TSimpleMovingAverage config) : base(config)
        {
            periodsD = periods = config.Periods;
        }

        #endregion

        readonly int periods;
        readonly double periodsD;

        public override Task CalculateIndex(int index)
        {
            var periods = Template.Periods;

            double avg = Result[index - 1];

            if (!double.IsNaN(avg))
            {
                avg = ((Template.Periods - 1) * avg + DataSeries[index]) / Template.Periods;
            }
            else
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
