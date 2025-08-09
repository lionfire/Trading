using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators;


//public class IndicatorMachineOptions
//{
//    public bool AllowTimeGaps { get; set; }
//}

//public class IndicatorMachine
//{
//    #region IndicatorParameters

//    public IndicatorMachineOptions? Options { get; }

//    #endregion
//}

public interface IIndicatorTimeSeriesComponent
{
    //public DateTime Start { get; }
    //public DateTime EndExclusive { get; }
    //Type ValueType { get; }
}

//public interface IIndicatorInput
//{
//    Type Type { get; }
//    bool TimeIndexed { get; }
//    bool TimeGaps { get; }
//}


public abstract class IndicatorTimeSeriesComponent<T> : IIndicatorTimeSeriesComponent
{
    TimeFrameValuesWindowWithGaps<T> window;

    public IndicatorTimeSeriesComponent(uint capacity, TimeFrame timeFrame)
    {
        window = new(capacity, timeFrame);
    }

    //public abstract DateTime Start { get; protected set; }
    //public abstract DateTime EndExclusive { get; protected set; }
}

public class IndicatorInput<T> : IndicatorTimeSeriesComponent<T>
{
    public IndicatorInput(uint lookbackPeriod, TimeFrame timeFrame) : base(lookbackPeriod, timeFrame)
    {
    }
}

public class IndicatorOutput<T> : IndicatorTimeSeriesComponent<T>
{
    public IndicatorOutput(uint memory, TimeFrame timeFrame) : base(memory, timeFrame)
    {
    }
}
