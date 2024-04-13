//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public sealed class TTriangleMovingAverage : TSingleSeriesIndicator
    {
        public TTriangleMovingAverage()
        {
            Periods = 14;
        }
    }

    public sealed class TriangleMovingAverage : SingleSeriesIndicatorBase<TTriangleMovingAverage>, IMovingAverageIndicator
    {
        public MovingAverageType Kind { get { return MovingAverageType.Triangular; } }

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

        public TriangleMovingAverage(TTriangleMovingAverage config) : base(config)
        {
            periods = config.Periods;
        }

        #endregion

        readonly int periods;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            sma = EffectiveIndicators.MovingAverage(MovingAverageType.Simple, periods, Template.IndicatorBarComponent, this.DataSeries);

        }

        IMovingAverageIndicator sma;

        public override Task CalculateIndex(int index)
        {
            /*
             * How To Calculate a Triangular Simple Moving Average
             * http://etfhq.com/blog/2010/11/09/triangular-simple-moving-average/
             * 
             * TriS-MA = SUM(MA1,L) / L
             * 
             * Where:
             * 
             * MA1 = SUM(CLOSE,L) / L
             * L = ceiling((n+1) / 2)
             * n = Number of Periods
             * 
             */
             // TOVERIFY - This is the simple calculation. There is also a weighted calculation.  Which one does cAlgo use?

            double avg = 0;
            for (int i = 0; i < periods; i++)
            {
                avg += sma.Result.Last(i);
            }

            avg /= periods;

            Result[index] = avg;

            return Task.CompletedTask;
        }
    }
}
