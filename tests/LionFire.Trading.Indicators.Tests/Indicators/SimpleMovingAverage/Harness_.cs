
//namespace LionFire.Trading.Indicators.Harnesses.Tests;

using LionFire.Trading.Indicators;

namespace SMA;

public class Harness_ : BinanceDataTest
{

    [Fact]
    public async void Verify_()
    {
        var h = new BufferingIndicatorHarness<SimpleMovingAverage, PSimpleMovingAverage<double>, double, double>(ServiceProvider, new(3)
        {
            TimeFrame = TimeFrame.m1,
            Signals = new[] { new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close) }
        });

        var result = await h.TryGetReverseValues(
            new DateTimeOffset(2024, 1, 1, 0, 2, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 10, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(8, result.Values.Count);

        var tolerance = 0.000_01;

        // InputSignal   | Average
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
        Assert.Equal(42453.3, result.Values[0], tolerance);
        Assert.Equal(42460.43333, result.Values[1], tolerance);
        Assert.Equal(42455.63333, result.Values[2], tolerance);
        Assert.Equal(42450.06667, result.Values[3], tolerance);
        Assert.Equal(42429.26667, result.Values[4], tolerance);
        Assert.Equal(42401.03333, result.Values[5], tolerance);
        Assert.Equal(42372.13333, result.Values[6], tolerance);
        Assert.Equal(42347.5, result.Values[7], tolerance);

    }

    [Fact]
    public async void Long_()
    {
        var h = new BufferingIndicatorHarness<SimpleMovingAverage, PSimpleMovingAverage<double>, double, double>(ServiceProvider, new(3)
        {
            TimeFrame = TimeFrame.m1,
            Signals = new[] { new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close) }
        });

        var result = await h.TryGetReverseValues(
            new DateTimeOffset(2019, 10, 1, 0, 2, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 10, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(2_236_328, result.Values.Count); // TODO: Verify expected count is correct

        var tolerance = 0.000_01;

        // InputSignal   | Average
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
        Assert.Equal(42453.3, result.Values[0], tolerance);
        Assert.Equal(42460.43333, result.Values[1], tolerance);
        Assert.Equal(42455.63333, result.Values[2], tolerance);
        Assert.Equal(42450.06667, result.Values[3], tolerance);
        Assert.Equal(42429.26667, result.Values[4], tolerance);
        Assert.Equal(42401.03333, result.Values[5], tolerance);
        Assert.Equal(42372.13333, result.Values[6], tolerance);
        Assert.Equal(42347.5, result.Values[7], tolerance);

    }

    [Fact]
    public async void Long_Period200_()
    {
        var h = new BufferingIndicatorHarness<SimpleMovingAverage, PSimpleMovingAverage<double>, double, double>(ServiceProvider, new(200)
        {
            TimeFrame = TimeFrame.m1,
            Signals = new[] { new SymbolValueAspect("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close) }
        });

        var result = await h.TryGetReverseValues(
            new DateTimeOffset(2019, 10, 1, 0, 2, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 10, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(2_236_328, result.Values.Count); // TODO: Verify expected count is correct

        //var tolerance = 0.000_01;

        //Assert.Equal(42453.3, result.Values[0], tolerance);
        //Assert.Equal(42460.43333, result.Values[1], tolerance);
        //Assert.Equal(42455.63333, result.Values[2], tolerance);
        //Assert.Equal(42450.06667, result.Values[3], tolerance);
        //Assert.Equal(42429.26667, result.Values[4], tolerance);
        //Assert.Equal(42401.03333, result.Values[5], tolerance);
        //Assert.Equal(42372.13333, result.Values[6], tolerance);
        //Assert.Equal(42347.5, result.Values[7], tolerance);

    }

    [Fact]
    public async void Long_Period2000_()
    {
        var h = new BufferingIndicatorHarness<SimpleMovingAverage, PSimpleMovingAverage<double>, double, double>(ServiceProvider, new(2000)
        {
            TimeFrame = TimeFrame.m1,
            Signals = new[] { new SymbolValueAspect("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close) }
        });

        var result = await h.TryGetReverseValues(
            new DateTimeOffset(2019, 10, 1, 0, 2, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 10, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(2_236_328, result.Values.Count); // TODO: Verify expected count is correct

        //var tolerance = 0.000_01;

        //Assert.Equal(42453.3, result.Values[0], tolerance);
        //Assert.Equal(42460.43333, result.Values[1], tolerance);
        //Assert.Equal(42455.63333, result.Values[2], tolerance);
        //Assert.Equal(42450.06667, result.Values[3], tolerance);
        //Assert.Equal(42429.26667, result.Values[4], tolerance);
        //Assert.Equal(42401.03333, result.Values[5], tolerance);
        //Assert.Equal(42372.13333, result.Values[6], tolerance);
        //Assert.Equal(42347.5, result.Values[7], tolerance);

    }
}
