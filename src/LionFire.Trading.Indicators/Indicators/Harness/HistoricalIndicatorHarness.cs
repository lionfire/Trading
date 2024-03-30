using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harness;

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
public class HistoricalIndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IndicatorExecutorBase<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{

    #region Lifecycle

    public HistoricalIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options) : base(serviceProvider, options)
    {
    }

    #endregion

    public override ValueTask<IValuesResult<TOutput>> TryGetReverseOutput(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        return HistoricalIndicatorExecutorX<TIndicator, TParameters, TInput, TOutput>.TryGetReverseOutput(new TimeFrameRange(TimeFrame, start, endExclusive), Indicator, Inputs);
    }
}

