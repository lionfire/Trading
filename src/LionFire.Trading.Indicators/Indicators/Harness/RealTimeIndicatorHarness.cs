using LionFire.Trading.HistoricalData.Retrieval;
using System.Threading.Channels;

namespace LionFire.Trading.Indicators.Harness;

public class RealTimeIndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{

    public static void Example()
    {
        var sourceRef = new SymbolValueAspect("Binance", "Futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close);

        var indicator = new IndicatorHarness<SimpleMovingAverage>(55, [sourceRef]);

        var inputs = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var period = 3;
        var result = SimpleMovingAverage.Compute(period, inputs);
        Console.WriteLine(string.Join(", ", result));
    }

    #region State

    List<Channel> inputChannels;

    #endregion

    #region Lifecycle

    public RealTimeIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, IBars bars, ILastBars lastBars, IInputResolver inputResolver) : base(serviceProvider, options, bars, inputResolver)
    {
    }

    #endregion

}

