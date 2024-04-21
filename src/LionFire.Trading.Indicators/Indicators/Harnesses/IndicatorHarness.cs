using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

public interface IIndicatorHarness
{
    TimeFrame TimeFrame { get; }

}
public interface IIndicatorHarness<TOutput> : IIndicatorHarness
{
    ValueTask<IValuesResult<TOutput>> GetReverseOutput(DateTimeOffset start, DateTimeOffset endExclusive);
    ValueTask<IValuesResult<TOutput>> TryGetReverseOutput(DateTimeOffset start, DateTimeOffset endExclusive);
}
public interface IIndicatorHarness<TParameters, TInput, TOutput> : IIndicatorHarness<TOutput>
{
    TParameters Parameters { get; }
    IServiceProvider ServiceProvider { get; }

    Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);
}

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

    public IndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, OutputComponentOptions outputOptions)
    {
        ServiceProvider = serviceProvider;
        OutputExecutionOptions = outputOptions;
        OutputComponentOptions.FallbackToDefaults(outputOptions);
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
    public TimeFrameValuesWindowWithGaps<TOutput> OutputBuffer { get; protected set; }

    #endregion

    #region Methods

    public virtual async ValueTask<IValuesResult<TOutput>> GetReverseOutput(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var result = await TryGetReverseOutput(start, endExclusive);
        if (result == null)
        {
            throw new Exception("Failed to get output");
        }
        return result;
    }

    public abstract ValueTask<IValuesResult<TOutput>> TryGetReverseOutput(DateTimeOffset start, DateTimeOffset endExclusive);

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
}


