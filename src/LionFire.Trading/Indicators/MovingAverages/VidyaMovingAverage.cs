//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public sealed class TVidyaMovingAverage : TSingleSeriesIndicator
    {
        public TVidyaMovingAverage()
        {
            Periods = 14;
        }

        public double Sigma { get; set; } = 0.65;
    }

    public sealed class VidyaMovingAverage : SingleSeriesIndicatorBase<TVidyaMovingAverage>, IMovingAverageIndicator
    {
        public MovingAverageType Kind { get { return MovingAverageType.VIDYA; } }

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

        public VidyaMovingAverage(TVidyaMovingAverage config) : base(config)
        {
        }

        #endregion


        // http://etfhq.com/blog/2011/02/22/variable-moving-average-vma-volatility-index-dynamic-average-vidya/
        public override Task CalculateIndex(int index)
        {
            // α = 2 / (N + 1)
            // VMA = (α * VI * Close) + ((1 – (α * VI)) *VMA[1])


            throw new NotImplementedException();
            /*      var periods = Template.Periods;

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
        }
    }
}
