using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IDataSeries : ISeries<double>
    {
    }
    public interface ITimeSeries : ISeries<DateTime>
    {
    }

    public interface IBarSeries : ISeries<TimedBarStruct>
    {
    }

    public interface ISeries<T>
    {
        T this[int index] { get; set; }

        int Count { get; }

        T LastValue { get; }

        T Last(int index);
    }
}
