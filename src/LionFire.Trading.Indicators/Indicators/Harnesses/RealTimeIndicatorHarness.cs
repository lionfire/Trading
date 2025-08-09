using LionFire.Trading.HistoricalData.Retrieval;
using System.Threading.Channels;

namespace LionFire.Trading.Indicators.Harnesses;

#if FUTURE
public static class Examples
{
    public static void Example()
    {
        var sourceRef = new SymbolValueAspect("Binance", "Futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close);

        IServiceProvider sp = null!;

        var indicator = ActivatorUtilities.CreateInstance<IndicatorHarness<SimpleMovingAverage, uint, double, double>>(sp, new IndicatorHarnessOptions<uint>
        {
            TimeFrame = TimeFrame.m1,
            Parameters = 55,
            InputReferences = new[] { sourceRef },
        });
    }
}

// TODO: Subscribable
// TODO: MaxMemory: allow 

public class RealTimeIndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{

    #region State

    //List<Channel<int>> inputChannels;

    #endregion

    #region Lifecycle

    public RealTimeIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, IBars bars, ILastBars lastBars, IInputResolver inputResolver) : base(serviceProvider, options, bars, inputResolver)
    {
    }

    #endregion

}
#endif
