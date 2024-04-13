using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

public abstract class IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator2<TParameters, TInput, TOutput>
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    //public IBars Bars { get; }

    #endregion

    #region Identity (incl. Parameters)

    public TParameters Parameters { get; }
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    public IndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options)
    {
        ServiceProvider = serviceProvider;
        //Bars = bars;

        IndicatorHarnessOptions<TParameters>.FallbackToDefaults(options);
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
        return ActivatorUtilities.CreateInstance<TIndicator>(ServiceProvider, Parameters!);
        //return TIndicator.Create<TIndicator>(Parameters);
    }

    #endregion

    #region State

    protected TIndicator Indicator { get; init; }
    protected IReadOnlyList<IHistoricalTimeSeries> Inputs { get; init; }

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

    #endregion
}


