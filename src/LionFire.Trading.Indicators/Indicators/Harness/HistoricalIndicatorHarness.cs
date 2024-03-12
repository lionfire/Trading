using LionFire.Trading.HistoricalData.Retrieval;

namespace LionFire.Trading.Indicators.Harness;

public static class HistoricalIndicatorHarnessSample
{
    public static Task<IEnumerable<TOutput>?> Example<TOutput>(DateTimeOffset first, DateTimeOffset last)
    {
        var sourceRef = new SymbolValueAspect("Binance", "Futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close);

        IServiceProvider sp = null!;

        var options = new IndicatorHarnessOptions<uint>
        {
            InputReferences = [sourceRef],
            Parameters = 55, // MA period
            TimeFrame = TimeFrame.m1,
        };
        options = IndicatorHarnessOptions<uint>.FallbackToDefaults(options);

        var indicatorHarness = ActivatorUtilities.CreateInstance<HistoricalIndicatorHarness<SimpleMovingAverage, uint, double, double>>(sp, options);

        

        var values = indicatorHarness.TryGetReverseOutput(first, last);

        return values;
    }

}

public class HistoricalIndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{

    #region Lifecycle

    public HistoricalIndicatorHarness(IServiceProvider serviceProvider, IBars bars, IndicatorHarnessOptions<TParameters> options) : base(serviceProvider, FallbackToDefaults(options), bars)
    {
    }

    #endregion

}

