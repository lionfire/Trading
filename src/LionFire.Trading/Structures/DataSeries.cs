using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IndicatorDataSeries : IDoubleDataSeries
    {
    }

    public sealed class BarSeries : DataSeries<TimedBar>
        //, IBarSeries
    {
        public TimedBar UnsetValue { get { return TimedBar.Invalid; } }
    }

    public sealed class DataSeries : DataSeries<double>, IDoubleDataSeries, IndicatorDataSeries
    {
        public double UnsetValue { get { return double.NaN; } }
    }
   

}
