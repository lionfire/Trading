
using TIndicator = LionFire.Trading.Indicators.QuantConnect_.AverageTrueRange;
using TParameters = LionFire.Trading.Indicators.QuantConnect_.PAverageTrueRange;

//namespace LionFire.Trading.Indicators.Harnesses.Tests;

namespace QC.ATR;

public class Harness_ : BinanceDataTest
{
    [Fact]
    public async void _()
    {
        var h = new HistoricalIndicatorHarness<TIndicator, TParameters, IKline, decimal>(ServiceProvider, new()
        {
            Parameters = new TParameters
            {
                //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
                MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                Period = 14
            },
            TimeFrame = TimeFrame.h1,
            InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        });

        var result = await h.GetReverseOutput(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(5, result.Values.Count);

        //var tolerance = 0.000_01;

        // 2024.04.01 h1 14 Simple - verified to 0.1 via TradingView
        Assert.Equal(466.84285714285714285714285714M, result.Values[4]);
        Assert.Equal(529.78571428571428571428571429M, result.Values[3]);
        Assert.Equal(562.53571428571428571428571429M, result.Values[2]);
        Assert.Equal(605.25714285714285714285714286M, result.Values[1]);
        Assert.Equal(619.87142857142857142857142857M, result.Values[0]);

        // 2024.04.01 h1 14 Wilders - unverified
        //Assert.Equal(566.60124278574403522341881357M, result.Values[0]);
        //Assert.Equal(566.18595376926280716368179923M, result.Values[1]);
        //Assert.Equal(552.39256559766763848396501456M, result.Values[2]);
        //Assert.Equal(528.76122448979591836734693876M, result.Values[3]);
        //Assert.Equal(466.84285714285714285714285715M, result.Values[4]);


        // 2024.01.01 m1 - not verified
        //Assert.Equal(32.744554724222050336169453202M, result.Values[0]);
        //Assert.Equal(32.271058933777592669720949603M, result.Values[1]);
        //Assert.Equal(30.591909620991253644314868804M, result.Values[2]);
        //Assert.Equal(29.998979591836734693877551020M, result.Values[3]);
        //Assert.Equal(29.814285714285714285714285714M, result.Values[4]);

        // Input   | Average
        // 42331.9 | 42331.9  (premature)
        // 42350.4 | 42341.15 (premature)
        // 42360.2 | 42347.5
        // 42405.8 | 42372.13333 
        // 42437.1 | 42401.03333
        // 42444.9 | 42429.26667
        // 42468.2 | 42450.06667
        // 42453.8 | 42455.63333
        // 42459.3 | 42460.43333
        // 42446.8 | 42453.3

        // Assert.Equal(42331.9, double.NaN /*result.Values[-2] */, tolerance); //  (premature result)
        //Assert.Equal(42341.15, double.NaN /*result.Values[-1] */, tolerance);  //  (premature result)
        //Assert.Equal(42453.3M, result.Values[0]);
        //Assert.Equal(42460.43333, result.Values[1], tolerance);
        //Assert.Equal(42455.63333, result.Values[2], tolerance);
        //Assert.Equal(42450.06667, result.Values[3], tolerance);
        //Assert.Equal(42429.26667, result.Values[4], tolerance);
        //Assert.Equal(42401.03333, result.Values[5], tolerance);
        //Assert.Equal(42372.13333, result.Values[6], tolerance);
        //Assert.Equal(42347.5, result.Values[7], tolerance);

    }
}
