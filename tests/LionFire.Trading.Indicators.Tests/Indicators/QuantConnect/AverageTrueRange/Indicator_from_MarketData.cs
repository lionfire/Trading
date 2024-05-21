using LionFire.Trading.Indicators.QuantConnect_;

namespace QC.ATR;

public class Indicator_from_MarketData : BinanceDataTest
{
    [Fact]
    public async void _()
    {
        var historicalTimeSeries = Resolve<decimal>(
            new IndicatorHarnessOptions<PAverageTrueRange<decimal>>(new PAverageTrueRange<decimal>
            {
                Period = 14,
                MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
            })
            {
                TimeFrame = TimeFrame.h1,
                Inputs = new[] { 
                    new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) 
                } // OPTIMIZE - Aspect: HLC
            });


        //var h = new BufferingIndicatorHarness<TIndicator, PAverageTrueRange<TOutput>, IKline, decimal>();

        var result = await historicalTimeSeries.Get(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
    new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Items);
        Assert.Equal(5, result.Items.Count);

        // 2024.04.01 h1 14 Simple - verified to 0.1 via TradingView
        Assert.Equal(466.84285714285714285714285714M, result.Items[0]);
        Assert.Equal(529.78571428571428571428571429M, result.Items[1]);
        Assert.Equal(562.53571428571428571428571429M, result.Items[2]);
        Assert.Equal(605.25714285714285714285714286M, result.Items[3]);
        Assert.Equal(619.87142857142857142857142857M, result.Items[4]);
    }
}
