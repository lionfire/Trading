using LionFire.Trading.Data;
using LionFire.Trading.DataFlow;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

public static class HistoricalIndicatorHarness
{
    public static IHistoricalTimeSeries Create(
        IIndicatorParameters pIndicator,
        TimeFrame timeFrame,
        IReadOnlyList<IHistoricalTimeSeries> signals,
        OutputComponentOptions? outputExecutionOptions = null
        )
    {
        var type = typeof(HistoricalIndicatorHarness<,,,>).MakeGenericType(
                                                pIndicator.InstanceType,
                                                pIndicator.GetType(),
                                                pIndicator.InputType,
                                                pIndicator.OutputType
                                            );

        return (IHistoricalTimeSeries)Activator.CreateInstance(type, pIndicator,
                                    timeFrame,
                                    signals,
                                    outputExecutionOptions)!;
    }
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Ignored:
/// - IndicatorHarnessOptions.Memory
/// </remarks>
/// <typeparam name="TIndicator"></typeparam>
/// <typeparam name="TParameters"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public class HistoricalIndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    : IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator2<TParameters, TInput, TOutput>
    where TParameters : IIndicatorParameters
{
    #region Lifecycle

    public HistoricalIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, OutputComponentOptions? outputExecutionOptions = null) : base(serviceProvider, options, outputExecutionOptions)
    {
    }

    public HistoricalIndicatorHarness(
        TParameters parameters,
        TimeFrame timeFrame,
        IReadOnlyList<IHistoricalTimeSeries> inputs,
        OutputComponentOptions? outputExecutionOptions = null) : base(parameters, timeFrame, inputs, outputExecutionOptions)
    {
    }

    #endregion

    #region Methods

    #region Input

    // TODO: Move this out of this class. Instead, have OnInput(inputId, data), and have something else push to this indicator
    public override async Task<ArraySegment<TInput>> GetInputData(IReadOnlyList<IHistoricalTimeSeries> inputs, DateTimeOffset start, DateTimeOffset endExclusive)
    {
        IHistoricalTimeSeries<TInput> input;
        if (inputs[0].GetType().IsAssignableTo(typeof(IHistoricalTimeSeries<TInput>)))
        {
            input = (IHistoricalTimeSeries<TInput>)inputs[0];
        }
        else
        {
            input = (IHistoricalTimeSeries<TInput>)Activator.CreateInstance(typeof(HistoricalTimeSeriesTypeAdapter<,>).MakeGenericType(inputs[0].ValueType, typeof(TInput)), inputs[0])!;
        }

        var data = await input.Get(start, endExclusive).ConfigureAwait(false);

        if (!data.IsSuccess || data.Values.Any() != true) throw new Exception($"Failed to get data: {data.FailReason}");

        return data.Values;
    }

    #endregion

    #region Output

    DateTimeOffset nextExpectedStart = default;

    /// <summary>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="endExclusive"></param>
    /// <param name="outputBuffer"></param>
    /// <returns></returns>
    public override ValueTask<HistoricalDataResult<TOutput>> TryGetValues(DateTimeOffset start, DateTimeOffset endExclusive, ref TOutput[]? outputBuffer)
    {
        // OLD docs, if there's no ref outputBuffer:
        // Caller should check the ArraySegment's array: if it's different than outputBuffer, then it may be a new larger buffer that the caller may want to hold onto for future use

        // TOTELEMETRY - invocation, to help assess percentages of key events

        #region Output Buffer

        var outputCount = GetOutputCount(start, endExclusive);

        if (outputBuffer == null || outputBuffer.Length < outputCount)
        {
            outputBuffer = new TOutput[outputCount];
            // TOTELEMETRY - recreate larger buffer
        }
        // TOTELEMETRY - buffer is too big by a large amount, e.g. > 40% (may indicate excess/abnormal memory use, or maybe it's just a dead time in the market and it has no data but it will pick up again soon.)

        #endregion

        #region Determine start point

        var reuseIndicatorState = nextExpectedStart == start;
        var lookbackAmount = reuseIndicatorState ? 0 : Math.Max(0, Indicator.MaxLookback - 1);
        var inputStart = reuseIndicatorState
            ? start
            : TimeFrame.AddBars(start, -lookbackAmount);

        if (!reuseIndicatorState) Indicator.Clear();

        // TOTELEMETRY - reuseIndicatorState

        #endregion

        var outputBufferCopy = outputBuffer;
        return new ValueTask<HistoricalDataResult<TOutput>>(Task.Run<HistoricalDataResult<TOutput>>(async () =>
        {
            #region Input sources

            ArraySegment<TInput> inputData = await this.GetInputData(Inputs, inputStart, endExclusive).ConfigureAwait(false);

            #endregion

            //// OPTIMIZE: Avoid subscription, and return TOutput from OnNextFromArray
            //var lookbackAmountRemaining = lookbackAmount;
            //int outputIndex = 0;

            //IDisposable subscription = Indicator.Subscribe(o =>
            //{
            //    if (lookbackAmountRemaining-- > 0) return;
            //    outputBuffer[outputIndex++] = o;
            //});

            #region Calculate   

            await Task.Run(() => Indicator.OnBarBatch(inputData, outputBufferCopy, outputSkip: lookbackAmount));

#if OLD // one value at a time
        for (int i = 0; i < inputData.Length; i++)
        {
            Indicator.OnNextFromArray(inputData, i);

            // OPTIMIZE:
            // - send entire array, and
            // -  have indicator write into the buffer,
            // - either skipping lookbackAmount,
            //   - or keeping track of whether it's ready to write real values, and returning the total amount of valid values returned
            if (i >= lookbackAmount)
            {
                outputBuffer[i - lookbackAmount] = result;
            }
        }
#endif

            #endregion

            #region Finalize, and results

            nextExpectedStart = endExclusive;
            var outputArraySegment = new ArraySegment<TOutput>(outputBufferCopy, 0, (int)outputCount);
            //return new ArraySegmentValueResult<TOutput>(outputArraySegment); // OLD
            return new HistoricalDataResult<TOutput>(outputArraySegment);

            #endregion
        }));
    }

    #endregion

    #endregion

}

