using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

public abstract class IndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IIndicatorHarness<TParameters, TInput, TOutput> where TIndicator : IIndicator2<TParameters, TInput, TOutput>
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Identity (incl. Parameters)

    public TParameters Parameters { get; }
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Parameters

    public OutputComponentOptions OutputExecutionOptions { get; }

    #endregion

    #region Lifecycle

    public IndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, OutputComponentOptions? outputOptions = null)
    {
        ServiceProvider = serviceProvider;
        OutputExecutionOptions = outputOptions ?? new();
        OutputComponentOptions.FallbackToDefaults(OutputExecutionOptions);
        Parameters = options.Parameters;
        TimeFrame = options.TimeFrame;
        Indicator = CreateIndicator();

        List<IHistoricalTimeSeries> inputs = new(options.InputReferences.Length);

        var marketDataResolver = ServiceProvider.GetRequiredService<IMarketDataResolver>();
        foreach (var input in options.InputReferences)
        {
            inputs.Add(marketDataResolver.Resolve(input));
        }
        Inputs = inputs;

    }

    TIndicator CreateIndicator()
    {
        // OPTIMIZE
        //if(Indicator needs ServiceProvider){
        //return TIndicator.Create<TIndicator>(Parameters);
        //} else
        //{
        return ActivatorUtilities.CreateInstance<TIndicator>(ServiceProvider, Parameters!);
        //}
    }

    #endregion

    #region State

    protected TIndicator Indicator { get; init; }
    protected IReadOnlyList<IHistoricalTimeSeries> Inputs { get; init; }

    #endregion

    #region Methods

    #region Input

    //public abstract Task<(object, int)> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);
    public abstract Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);

    //public override async Task<(IReadOnlyList<InputSlot>, int)> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var d1 = GetData<TInput1>(sources[0], start, endExclusive);
    //    var d2 = GetData<TInput2>(sources[1], start, endExclusive);
    //    await Task.WhenAll(d1, d2);

    //    int count = d1.Result.count;
    //    if (count != d2.Result.count) throw new ArgumentException("InputSignal data counts do not match");

    //    return (new object[] { d1.Result.data, d2.Result.data }, count);
    //}

    #endregion

    #region Output

    //public abstract ValueTask<IValuesResult<TOutput>> TryGetValues(bool reverse, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null);
    public abstract ValueTask<IValuesResult<TOutput>> TryGetValues(DateTimeOffset start, DateTimeOffset endExclusive, ref TOutput[] outputBuffer);

    protected uint GetOutputCount(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var outputCount = (uint)TimeFrame.GetExpectedBarCount(start, endExclusive)!.Value;
        if (outputCount < 0) throw new ArgumentOutOfRangeException(nameof(endExclusive), "Invalid date range");
        return outputCount;
    }

    #endregion


    #endregion
}


