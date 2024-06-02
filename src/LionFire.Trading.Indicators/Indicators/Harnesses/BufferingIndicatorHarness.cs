using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

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
public class BufferingIndicatorHarness<TIndicator, TParameters, TInput, TOutput> 
    : HistoricalIndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator2<TParameters, TInput, TOutput>
    where TParameters : IIndicatorParameters
{
    #region Configuration

    /// <summary>
    /// Fast-forward: Continue with current indicator state even though some inputs are not requested (will be wasted).  It may typically make sense to match this value with the MaxLookback on the InputSignals, or at least the computationally expensive inputs.
    /// </summary>
    public static uint MaxFastForwardBars = 1;

    #endregion

    #region Lifecycle

    public BufferingIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, OutputComponentOptions? outputExecutionOptions = null) : base(serviceProvider, options, outputExecutionOptions)
    {


    }

    #endregion

    #region State

    /// <summary>
    /// (optional) A common output buffer for the indicator.
    /// If not present, the harness will have no memory of recently computed values.
    /// </summary>
    /// <remarks>
    /// Output buffering scenarios:
    /// - Real-time: the last several bars may be desired
    /// - Historical (backtesting): we will be backtesting large chunks of data, and returning chunks.  There will be no need for a separate buffer.
    /// 
    /// Implications:
    /// - random access: the common buffer will be bypassed if it doesn't align
    /// - chaotic fast forward: the common buffer will be either fast-forwarded, or restarted at some point in the future
    /// 
    /// Suggestions:
    /// - Backtesting: do not set a buffer.  Have a chunk cache manager that retains chunks, accommodating large lookback requirements if necessary.
    /// - Real-time: set a buffer according to what the attached bot needs.
    /// - Visual: set a buffer according to what the user's screen typically displays.
    /// </remarks>
    public TimeFrameValuesWindowWithGaps<TOutput>? OutputBuffer { get; protected set; }

    #endregion

    public Task<IValuesResult<TOutput>> TryGetReverseValues(DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
    {
        // ENH: Also ThrowFailReason/warn if the current bar isn't ready/finalized yet, which may take about 5 seconds to be confident of.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(endExclusive, DateTimeOffset.UtcNow);

        return _TryGetValues(start, endExclusive, outputBuffer: outputBuffer, reverse: true);
    }

    // TODO: Return HistoricalDataResult<TOutput>?
    private async Task<IValuesResult<TOutput>> _TryGetValues(
        DateTimeOffset start, DateTimeOffset endExclusive,
        bool reverse,
        //TValue parameters,
        uint? maxFastForwardBars = null, // If memory.LastOpenTime is within this many bars from inputStart, calculate to bring memory up to speed with desired range.EndExclusive
        TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null,
        bool skipAhead = false,
        bool noCopy = true,
        DateChunker? historicalDataChunkRangeProvider = null,
        bool allowCreateOwnBuffer = true
        )
    {
        #region Output Buffer

        var outputCount = GetOutputCount(start, endExclusive);

        TimeFrameRange range = new TimeFrameRange(TimeFrame, start, endExclusive);

        // Condition: when reusing memory, memory capacity is lower than outputCount:
        if(outputBuffer != null && outputBuffer.Capacity < outputCount) { outputBuffer = null; }

        #endregion

        //if (range.TimeFrame.TimeSpan < TimeSpan.Zero) throw new NotImplementedException("Irregular TimeFrames"); // This exception is getting moved into TimeFrame date calculation methods

        #region Init outputBuffer, inputStart

        //TimeFrameValuesWindowWithGaps<TOutput>? actualMemory = null;
        bool reusingMemory;

        var inputStart = TimeFrame.AddBars(start, -(Math.Max(0, Indicator.MaxLookback - 1)));

        // REVIEW: control paths for reusingMemory (UNUSED), outputBuffer != null and Indicator.Clear() are complicated and could perhaps be refactored
        if (outputBuffer == null)
        {
            reusingMemory = false;

            if (allowCreateOwnBuffer)
            {
                outputBuffer = OutputBuffer = new TimeFrameValuesWindowWithGaps<TOutput>(outputCount, TimeFrame, range.Start);
            }
        }
        else
        {
            var outputBufferNextStart = outputBuffer.NextExpectedOpenTime;

            if (range.Start == outputBufferNextStart)
            {
                reusingMemory = true;

                if (Indicator.IsReady)
                {
                    inputStart = outputBufferNextStart; // Continue where memory left off
                }
                else
                {
                    Indicator.Clear();
                }
            }
            else if (range.Start < outputBuffer.FirstOpenTime)
            {
                // Scenario: start < memory.Start: create own memory, create own indicator (which has its own buffer)
                reusingMemory = false;
                outputBuffer = null;
            }
            else if (range.Start > outputBufferNextStart) // Possibly fast-forward
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
                        outputBuffer = null;
                    }
                }
                else
                {
                    reusingMemory = true;
                    inputStart = outputBufferNextStart; // Fast-forward existing memory and buffer
                }
            }
            else // range.Start >= outputBuffer.FirstOpenTime && range.Start < outputBufferNextStart
            {
                reusingMemory = true;
                inputStart = outputBufferNextStart;
            }

            if(outputBuffer == null) { Indicator.Clear(); }
        }

        //actualMemory = reusingMemory ? outputBuffer! : new TimeFrameValuesWindowWithGaps<TOutput>(outputCount, range.TimeFrame, range.Start - range.TimeFrame.TimeSpan);
        outputBuffer ??= new TimeFrameValuesWindowWithGaps<TOutput>(outputCount, range.TimeFrame, range.Start);

        #endregion

        #region Input sources

        ArraySegment<TInput> inputData = await this.GetInputData(Inputs, inputStart, range.EndExclusive).ConfigureAwait(false);

        #endregion

        // OPTIMIZE: Avoid subscription, and have a callback in an OnNext overload
        IDisposable subscription = reverse
            ? Indicator.Subscribe(o => outputBuffer.PushFront(o))
            : Indicator.Subscribe(o => outputBuffer.PushBack(o));

        #region Calculate   

        Indicator.OnNext(inputData);

        #endregion

        subscription.Dispose();

        #region return result

        //if (separateOutputBuffer != null)
        //{
        //    return new ListValueResult<TOutput>(separateOutputBuffer);
        //}
        //else
        if (noCopy)
        {
            // TODO: Use HistoricalDataResult?
            return new ArraySegmentsValueResult<TOutput>(outputBuffer.ValuesBuffer);
        }
        else
        {
            // TODO: Use HistoricalDataResult?
            return new ListValueResult<TOutput>(outputBuffer.ToArray(outputCount));
        }

        #endregion

    }
}

#if false
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

//        var indicatorHarness = ActivatorUtilities.CreateInstance<BufferingIndicatorHarness<SimpleMovingAverage, uint, double, double>>(sp, options);

//        var values = indicatorHarness.TryGetReverseValues(first, last);

//        return values;
//    }
//}
#endif