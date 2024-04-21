#if false // OLD
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.ValueWindows;
using System.Reactive;
using System.Runtime.InteropServices;
using LionFire.Trading.HistoricalData;

namespace LionFire.Trading.Indicators.Harnesses;

public static class HistoricalIndicatorExecutorX<TIndicator, TParameters, TInput, TOutput>
      where TIndicator : IIndicator2<TParameters, TInput, TOutput>
{
  

#if TODO
    void TODO()
    {

        var input = inputs[0].GetReverse(inputStart, memory.LastOpenTime);
        indicator.OnNext(input);


        //var valuesNeeded = outputCount + indicator.Lookback;

        //bool reuseIndicatorIndicatorBuffer = false;


        if (memory.LastOpenTime > )
            if (memory != null && (memory.ValueCount == 0))
            {

                if ()
                {

                    reusingMemory = true;

                    valuesNeeded -= range.TimeFrame.ToExactBarCount(range.EndExclusive - actualMemory.LastOpenTime);

                    inputStart = range.TimeFrame.AddBar(actualMemory.LastOpenTime);
                    reuseIndicatorIndicatorBuffer = true;
                }
            }

        var actualMemory = memory;
        if (actualMemory == null || actualMemory.Capacity < outputCount)
        {
            reusingMemory = false;
            actualMemory = new TimeFrameValuesWindowWithGaps<TOutput>((int)outputCount, range.TimeFrame);
        }
        else

            var firstInputBar = range.TimeFrame.AddBars(inputStart, -indicator.Lookback);


        //memory.LastOpenTime = range.EndExclusive;




        var timeFrame = indicatorHarness.TimeFrame;

        var bars = serviceProvider.GetRequiredService<IBars>();

        var start = bars.GetBarIndexNearestOrBefore(first, timeFrame);
        var end = bars.GetBarIndexNearestOrBefore(last, timeFrame);

        var result = indicatorHarness.TryGetOutput(start, end);

        return result;
    }
#endif
}


#endif