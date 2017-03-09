using LionFire.Execution.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Execution.Jobs;

namespace LionFire.Trading.Data
{


    public class HistoricalDataJobPrioritizer : IJobPrioritizer
    {
        
        public double GetPriority(IJob job)
        {
            var dJob = job as LoadHistoricalDataJob;
            if (dJob == null) return double.NaN;

            double points = 0;

            var tf = dJob.MarketSeriesBase.TimeFrame;

            if (tf.Name == "h1")
            {
                points += 1000;
            }
            else if (tf.Name == "m1")
            {
                points += 100;
            }
            else if (tf.TimeSpan > TimeSpan.FromHours(1))
            {
                points += 500;
            }
            else if (tf.TimeFrameUnit == TimeFrameUnit.Tick)
            {
                points += -1000;
            }

            // FUTURE: Add points based on # of agents waiting on this data and their significance (cash at stake, urgency, etc.)

            return points;

        }
    }
}
