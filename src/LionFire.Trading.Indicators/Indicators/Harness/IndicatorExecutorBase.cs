using LionFire.Results;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harness;

public abstract class IndicatorExecutorBase<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public IBars Bars { get; }

    #endregion

    #region Identity (incl. Parameters)

    public TParameters Parameters { get; }
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    public IndicatorExecutorBase(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, IBars bars, IMarketDataResolver inputResolver)
    {
        ServiceProvider = serviceProvider;
        Bars = bars;

        IndicatorHarnessOptions<TParameters>.FallbackToDefaults(options);
        Parameters = options.Parameters;
        TimeFrame = options.TimeFrame;
        Indicator = CreateIndicator();

        List<IHistoricalTimeSeries> inputs = new(options.InputReferences.Length);

        foreach (var input in options.InputReferences)
        {
            inputs.Add(inputResolver.Resolve(input));
        }
        Inputs = inputs;
    }

    IIndicator<TParameters, TInput, TOutput> CreateIndicator()
    {
        var indicator = TIndicator.Create(Parameters);
        return indicator;
    }

    #endregion

    #region State

    protected IIndicator<TParameters, TInput, TOutput> Indicator { get; init; }
    protected IReadOnlyList<IHistoricalTimeSeries> Inputs { get; init; }

    #endregion

    #region Methods

    public virtual async ValueTask<IValueResult<IEnumerable<TOutput>>> GetReverseOutput(DateTimeOffset firstOpenTime, DateTimeOffset? lastOpenTime = null)
    {
        var result = await TryGetReverseOutput(firstOpenTime, lastOpenTime);
        if (result == null)
        {
            throw new Exception("Failed to get output");
        }
        return result;
    }

    public abstract ValueTask<IValueResult<IEnumerable<TOutput>>?> TryGetReverseOutput(DateTimeOffset firstOpenTime, DateTimeOffset? lastOpenTime = null);

    #endregion
}


