using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.ValueWindows;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Indicators.Harness;

public static class HistoricalIndicatorExecutorX<TIndicator, TParameters, TInput, TOutput>
      where TIndicator : IIndicator<TParameters, TInput, TOutput>
{
    public static uint MaxFastForwardBars = 1;

    public static async ValueTask<IValuesResult<TOutput>> TryGetReverseOutput(
        //IServiceProvider serviceProvider,
        SymbolBarsRange range,
        TIndicator indicator,
        //TParameters parameters,
        IReadOnlyList<IHistoricalTimeSeries> inputs,
        uint? maxFastForwardBars = null, // If memory.LastOpenTime is within this many bars from inputStart, calculate to bring memory up to speed with desired range.EndExclusive
        TimeFrameValuesWindowWithGaps<TOutput>? memory = null,
        bool skipAhead = false,
        bool noCopy
        )
    {

        #region outputCount

        var outputCount = (uint)range.TimeFrame.GetExpectedBarCount(range.Start, range.EndExclusive)!.Value;
        if (outputCount < 0) throw new ArgumentOutOfRangeException(nameof(range), "Invalid date range");

        #endregion

        //if (range.TimeFrame.TimeSpan < TimeSpan.Zero) throw new NotImplementedException("Irregular TimeFrames"); // This exception is getting moved into TimeFrame date calculation methods

        //uint actualMaxFastForwardBars = 0; // Value is only valid if it is in effect.  REVIEW - may not need to have this variable

        #region (local)

        uint ComputeActualMaxFastForwardBars() // Called in one place
            => maxFastForwardBars ?? indicator.DefaultMaxFastForwardBars ?? MaxFastForwardBars;

        #endregion

        #region Init actualMemory, inputStart

        TimeFrameValuesWindowWithGaps<TOutput> actualMemory;
        bool reusingMemory;
        var inputStart = range.TimeFrame.AddBars(range.Start, -indicator.Lookback);

        if (memory == null)
        {
            reusingMemory = false;
        }
        else if (range.Start < memory.FirstOpenTime)
        {
            // Scenario: start < memory.Start: create own memory, create own indicator (which has its own buffer)
            reusingMemory = false;
        }
        else if (range.Start > memory.LastOpenTime) // Possibly fast-forward
        {
            if (range.Start > range.TimeFrame.AddBars(memory.LastOpenTime, ComputeActualMaxFastForwardBars()))
            {
                // No fast-forward, because we would have to fast-forward too much

                if (skipAhead) // Use the memory buffer, but reset state
                {
                    // Scenario: lastOpenTime < inputStart: send full input to flush the buffer.
                    // Call clear here to notify subscribers,
                    //   - (though in theory, if we are pumping the memory and buffer full again past the Lookback amount,
                    //     it should be unnecessary to actually delete existing data.)
                    memory.Clear();
                    indicator.Clear();
                    reusingMemory = true;
                }
                else
                {
                    reusingMemory = false;
                }
            }
            else
            {
                reusingMemory = true;
                inputStart = range.TimeFrame.AddBar(memory.LastOpenTime); // Fast-forward existing memory and buffer
            }
        }
        else
        {
            reusingMemory = true;
            inputStart = range.TimeFrame.AddBar(memory.LastOpenTime); // Continue where memory left off
        }

        actualMemory = reusingMemory ? memory! : new TimeFrameValuesWindowWithGaps<TOutput>(outputCount, range.TimeFrame);

        #endregion

        #region separateOutputBuffer

        // Condition: when reusing memory, memory capacity is lower than outputCount:
        List<TOutput>? separateOutputBuffer = reusingMemory && actualMemory.Capacity < outputCount
            ? new List<TOutput>((int)outputCount)
            : null;

        #endregion

        #region Input sources

        #endregion

        #region Calculate   


        DateTimeOffset openTimeCursor = inputStart;
        var inputBarCount = range.TimeFrame.ToExactBarCount(range.EndExclusive - inputStart);

        var inputData = new T[inputs.Count][];

        for(int inputIndex = 0; inputIndex < inputs.Count; inputIndex++)
        {
            inputs[inputIndex].TryGetValueChunks
        }

        //for(int i = 0; i < inputBarCount; i++)
        //{
        //    var input = inputs[i].GetReverse(inputStart, memory.LastOpenTime);
        //    indicator.OnNext(input);
        //}



        #endregion

        #region return result

        if (separateOutputBuffer != null)
        {
            return new ListValueResult<TOutput>(separateOutputBuffer);
        }
        else if (noCopy)
        {
            return new ArraySegmentsValueResult<TOutput>(actualMemory.ReversedValuesBuffer);
        }
        else
        {
            return new ListValueResult<TOutput>(actualMemory.ToReverseArray(outputCount));
        }

        #endregion

    }

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

