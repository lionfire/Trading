//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public class TTimeSeriesMovingAverage : TSingleSeriesIndicator
    {
        public TTimeSeriesMovingAverage()
        {
            Periods = 14;
        }
    }

    // https://www.paritech.com.au/education/tech_anaylsis/trend/methods.html
    public class TimeSeriesMovingAverage : SingleSeriesIndicatorBase<TTimeSeriesMovingAverage>, IMovingAverageIndicator
    {
        public MovingAverageType Kind { get { return MovingAverageType.TimeSeries; } }

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

        public TimeSeriesMovingAverage(TTimeSeriesMovingAverage config) : base(config)
        {
        }

        #endregion

        // Implementation From http://ctdn.com/forum/calgo-support/11309?page=1#2
        // TOVERIFY vs cAlgo
        public override Task CalculateIndex(int index)
        {
            var periods = Template.Periods;
            //throw new NotImplementedException();
            /*      

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
                          avg /= periods;
                      }
                  }
                  Result[index] = avg;
                  return Task.CompletedTask;
              */

            double sumX = 0, sumY = 0, sumXY = 0, sumXPower = 0;

            for (int i = 0; i < periods; i++)
            {
                sumXPower += i * i;
                sumX += i;
                sumY += DataSeries[index - i];
                sumXY += i * DataSeries[index - i];
            }

            Result[index] = (sumXPower * sumY - sumX * sumXY) / (sumXPower * periods - sumX * sumX);
            return Task.CompletedTask;
        }
    }
}
