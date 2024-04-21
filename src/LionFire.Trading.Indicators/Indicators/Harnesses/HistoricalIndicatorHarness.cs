using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;
//using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;


namespace LionFire.Trading.Indicators.Harnesses;

//public static class HistoricalIndicatorHarnessSample
//{
//    public static ValueTask<IValuesResult<IEnumerable<double>>?> Example(DateTimeOffset first, DateTimeOffset last)
//    {
//        var sourceRef = new SymbolValueAspect("Binance", "Futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close);

//        IServiceProvider sp = null!;

//        var options = new IndicatorHarnessOptions<uint>
//        {
//            InputReferences = [sourceRef],
//            Parameters = 55, // MA period
//            TimeFrame = TimeFrame.m1,
//        };
//        //options = IndicatorHarnessOptions<uint>.FallbackToDefaults(options);

//        var indicatorHarness = ActivatorUtilities.CreateInstance<HistoricalIndicatorHarness<SimpleMovingAverage, uint, double, double>>(sp, options);

//        var values = indicatorHarness.TryGetReverseOutput(first, last);

//        return values;
//    }
//}


/// <remarks>
/// Planned optimizations:
/// - keep a memory of N most recent bars
/// - only calculate new portions necessary
/// - listen to the corresponding real-time indicator, if available, and append to memory
/// 
/// For now, this is the same as one-shot execution with no caching.
/// </remarks>
/// <typeparam name="TIndicator"></typeparam>
/// <typeparam name="TParameters"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public class HistoricalIndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator2<TParameters, TInput, TOutput>
{
    #region Configuration

    /// <summary>
    /// Fast-forward: Continue with current indicator state even though some inputs are not requested (will be wasted).  It may typically make sense to match this value with the MaxLookback on the Inputs, or at least the computationally expensive inputs.
    /// </summary>
    public static uint MaxFastForwardBars = 1;

    #endregion

    #region Lifecycle

    public HistoricalIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, OutputComponentOptions outputExecutionOptions) : base(serviceProvider, options, outputExecutionOptions)
    {
     
    }

    #endregion

    // TODO: Move this out of this class. Instead, have OnInput(inputId, data), and have something else push to this indicator
    public override async Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    {
        IHistoricalTimeSeries<TInput> source;
        if (sources[0].GetType().IsAssignableTo(typeof(IHistoricalTimeSeries<TInput>)))
        {
            source = (IHistoricalTimeSeries<TInput>)sources[0];
        }
        else
        {
            source = (IHistoricalTimeSeries<TInput>)Activator.CreateInstance(typeof(HistoricalTimeSeriesTypeAdapter<,>).MakeGenericType(sources[0].ValueType, typeof(TOutput)), sources[0])!;
        }
        var data = await source.Get(start, endExclusive).ConfigureAwait(false);

        if (!data.IsSuccess || data.Items?.Any() != true) throw new Exception("Failed to get data");

        return data.Items.ToArray(); // COPY
    }

    public override ValueTask<IValuesResult<TOutput>> TryGetReverseOutput(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        if(endExclusive > DateTimeOffset.UtcNow)
        {
            // ENH: Also Throw/warn if the current bar isn't ready/finalized yet
            throw new ArgumentOutOfRangeException(nameof(endExclusive));
        }
            
        return _TryGetReverseOutput(new TimeFrameRange(TimeFrame, start, endExclusive));
    }

    private async ValueTask<IValuesResult<TOutput>> _TryGetReverseOutput(
        TimeFrameRange range,
        //TParameters parameters,
        uint? maxFastForwardBars = null, // If memory.LastOpenTime is within this many bars from inputStart, calculate to bring memory up to speed with desired range.EndExclusive
        TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null,
        bool skipAhead = false,
        bool noCopy = true,
        HistoricalDataChunkRangeProvider? historicalDataChunkRangeProvider = null
        )
    {

        #region outputCount

        var outputCount = (uint)range.TimeFrame.GetExpectedBarCount(range.Start, range.EndExclusive)!.Value;
        if (outputCount < 0) throw new ArgumentOutOfRangeException(nameof(range), "Invalid date range");

        #endregion

        OutputBuffer = new TimeFrameValuesWindowWithGaps<TOutput>(outputCount, TimeFrame, range.Start - range.TimeFrame.TimeSpan);

        //if (range.TimeFrame.TimeSpan < TimeSpan.Zero) throw new NotImplementedException("Irregular TimeFrames"); // This exception is getting moved into TimeFrame date calculation methods

        #region Init actualMemory, inputStart

        TimeFrameValuesWindowWithGaps<TOutput> actualMemory;
        bool reusingMemory;

        var inputStart = range.TimeFrame.AddBars(range.Start, -(Math.Max(0, Indicator.MaxLookback - 1)));

        if (outputBuffer == null)
        {
            reusingMemory = false;
        }
        else if (range.Start < outputBuffer.FirstOpenTime)
        {
            // Scenario: start < memory.Start: create own memory, create own indicator (which has its own buffer)
            reusingMemory = false;
        }
        else if (range.Start > outputBuffer.LastOpenTime) // Possibly fast-forward
        {
            #region (local)

            uint ComputeActualMaxFastForwardBars() // Called in one place
                => maxFastForwardBars ?? Indicator.DefaultMaxFastForwardBars ?? MaxFastForwardBars;

            #endregion

            if (range.Start > range.TimeFrame.AddBars(outputBuffer.LastOpenTime, ComputeActualMaxFastForwardBars()))
            {
                // No fast-forward, because we would have to fast-forward too much

                if (skipAhead) // Use the memory buffer, but reset state
                {
                    // Scenario: lastOpenTime < inputStart: send full input to flush the buffer.
                    // Call clear here to notify subscribers,
                    //   - (though in theory, if we are pumping the memory and buffer full again past the Lookback amount,
                    //     it should be unnecessary to actually delete existing data.)
                    outputBuffer.Clear();
                    Indicator.Clear();
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
                inputStart = range.TimeFrame.AddBar(outputBuffer.LastOpenTime); // Fast-forward existing memory and buffer
            }
        }
        else
        {
            reusingMemory = true;
            inputStart = range.TimeFrame.AddBar(outputBuffer.LastOpenTime); // Continue where memory left off
        }

        actualMemory = reusingMemory ? outputBuffer! : new TimeFrameValuesWindowWithGaps<TOutput>(outputCount, range.TimeFrame, range.Start - range.TimeFrame.TimeSpan);

        #endregion

        #region separateOutputBuffer

        // Condition: when reusing memory, memory capacity is lower than outputCount:
        List<TOutput>? separateOutputBuffer = reusingMemory && actualMemory.Capacity < outputCount
            ? new List<TOutput>((int)outputCount)
            : null;

        #endregion

        #region Input sources

        TInput[] inputData = await this.GetInputData(Inputs, inputStart, range.EndExclusive).ConfigureAwait(false);

        #endregion

        var subscription = Indicator.Subscribe(o => actualMemory.PushFront(o)); // OPTIMIZE: Avoid subscription, and return TOutput from OnNextFromArray

        #region Calculate   


        for (int i = 0; i < inputData.Length; i++)
        {
            Indicator.OnNextFromArray(inputData, i); // OPTIMIZE - send entire array
        }

        #endregion

        subscription.Dispose();

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

}

