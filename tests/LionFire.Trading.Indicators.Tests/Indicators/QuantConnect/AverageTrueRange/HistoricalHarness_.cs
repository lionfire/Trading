using LionFire.Trading.Indicators.QuantConnect_;

namespace QC.ATR;

public class HistoricalHarness_ : BinanceDataTest
{
    [Fact]
    public async void forward_decimal()
    {
        var h = new HistoricalIndicatorHarness<AverageTrueRange<decimal>, PAverageTrueRange<decimal>, IKline, decimal>(ServiceProvider, new(new PAverageTrueRange<decimal>
        {
            MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
            Period = 14,
        })
        {
            //IndicatorParameters = ,
            TimeFrame = TimeFrame.h1,
            Inputs = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        });


        decimal[]? output = null;
        var result = await h.TryGetValues(
            new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero),
            ref output);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(5, result.Values.Count);

        // 2024.04.01 h1 14 Simple - verified to 0.1 via TradingView
        Assert.Equal(466.84285714285714285714285714M, result.Values[0]);
        Assert.Equal(529.78571428571428571428571429M, result.Values[1]);
        Assert.Equal(562.53571428571428571428571429M, result.Values[2]);
        Assert.Equal(605.25714285714285714285714286M, result.Values[3]);
        Assert.Equal(619.87142857142857142857142857M, result.Values[4]);
    }

    [Fact]
    public async void forward_double()
    {
        var h = new HistoricalIndicatorHarness<AverageTrueRange<double>, PAverageTrueRange<double>, IKline, double>(ServiceProvider, new(new PAverageTrueRange<double>
        {
            MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
            Period = 14,
        })
        {
            TimeFrame = TimeFrame.h1,
            Inputs = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        });


        double[]? output = null;
        var result = await h.TryGetValues(
            new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero),
            ref output);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(5, result.Values.Count);

        // 2024.04.01 h1 14 Simple - verified to forward_decimal unit test results
        Assert.Equal(466.8428571428571, result.Values[0]);
        Assert.Equal(529.78571428571433, result.Values[1]);
        Assert.Equal(562.53571428571422, result.Values[2]);
        Assert.Equal(605.25714285714287, result.Values[3]);
        Assert.Equal(619.87142857142851, result.Values[4]);
    }

    [Fact]
    public async void forward_float()
    {
        var h = new HistoricalIndicatorHarness<AverageTrueRange<float>, PAverageTrueRange<float>, IKline, float>(ServiceProvider, new(new PAverageTrueRange<float>
        {
            MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
            Period = 14,
        })
        {
            TimeFrame = TimeFrame.h1,
            Inputs = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        });


        float[]? output = null;
        var result = await h.TryGetValues(
            new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero),
            ref output);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(5, result.Values.Count);

        // 2024.04.01 h1 14 Simple - verified to forward_decimal unit test results
        Assert.Equal(466.842865f, result.Values[0]);
        Assert.Equal(529.7857f, result.Values[1]);
        Assert.Equal(562.5357f, result.Values[2]);
        Assert.Equal(605.257141f, result.Values[3]);
        Assert.Equal(619.8714f, result.Values[4]);
    }
}
