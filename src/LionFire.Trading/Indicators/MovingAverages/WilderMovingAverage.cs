////#define OptimizeSpeed
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Diagnostics;

//namespace LionFire.Trading.Indicators
//{

//    public class TWilderMovingAverage : TSingleSeriesIndicator
//    {
//        public TWilderMovingAverage()
//        {
//            Periods = 14;
//        }
//    }

//    // https://mahifx.com/mfxtrade/indicators/wilders-moving-average
//    public class WilderMovingAverage : SingleSeriesIndicatorBase<TWilderMovingAverage>, IMovingAverageIndicator
//    {
//        public MovingAverageType Kind { get { return MovingAverageType.Wilder; } }

//        #region Outputs

//        public DataSeries Result { get; private set; } = new DataSeries();

//        public override IEnumerable<IndicatorDataSeries> Outputs
//        {
//            get
//            {
//                yield return Result;
//            }
//        }

//        #endregion

//        #region Lifecycle

//        public WilderMovingAverage(TWilderMovingAverage config) : base(config)
//        {
//        }

//        #endregion

//        public override Task CalculateIndex(int index)
//        {
//            //  Wilder EMA formula = price today* K +EMA yesterday(1 - K) where K = 1 / N
//            var last = 

//            throw new NotImplementedException();
//            /*      var periods = Template.Periods;

//                  double avg = Result[index - 1];

//                  if (!double.IsNaN(avg))
//                  {
//                      avg = ((Template.Periods - 1) * avg + DataSeries[index]) / Template.Periods;
//                  }
//                  else
//                  {
//                      avg = 0;
//                      {
//                          for (int i = periods; i > 0; i--)
//                          {
//                              avg += DataSeries.Last(i);
//                          }
//                          avg /= periods;
//                      }
//                  }
//                  Result[index] = avg;
//                  return Task.CompletedTask;
//              */
//        }
//    }
//}
