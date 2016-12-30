//#define OptimizeSpeed
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LionFire.Trading.Indicators
{

    public class TSimpleMovingAverage : TSingleSeriesIndicator
    {
        public int Periods { get; set; } = 14;

        public BarComponent IndicatorBarSource { get; set; } = BarComponent.Close;
    }


    
    public class SimpleMovingAverage : SingleSeriesIndicatorBase<TSimpleMovingAverage>, IMovingAverageIndicator
    {

        #region Outputs

        public DoubleDataSeries Result { get; private set; } = new DoubleDataSeries();

        public override IEnumerable<IDataSeries> Outputs
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
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            
            
        }


        #endregion

        #region Derived Configuration

        

        #endregion

        protected override void CalculateIndex(int index)
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
                    avg /= periods;
                }
            }
            Result[index] = avg;
        }
    }
}
