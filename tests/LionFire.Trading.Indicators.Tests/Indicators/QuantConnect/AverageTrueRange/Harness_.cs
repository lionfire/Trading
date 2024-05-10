using LionFire.Persistence.Handles;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using TIndicator = LionFire.Trading.Indicators.QuantConnect_.AverageTrueRange;
using TParameters = LionFire.Trading.Indicators.QuantConnect_.PAverageTrueRange<decimal>;

//namespace LionFire.Trading.Indicators.Harnesses.Tests;

namespace QC.ATR;


public class HistoricalTimeSeriesFromIndicatorHarness<TValue> : IHistoricalTimeSeries<TValue>
{

    public HistoricalTimeSeriesFromIndicatorHarness()
    {
    }


    public Type ValueType => typeof(TValue);

    public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var h = new BufferingIndicatorHarness<TIndicator, TParameters, IKline, decimal>(ServiceProvider, new()
        {
            Parameters = new TParameters
            {
                MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                Period = 14,
            },
            TimeFrame = TimeFrame.h1,
            InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        });

        var valuesResult = await h.GetReverseValues(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        var result = new HistoricalDataResult<TValue>
        {
            Items = valuesResult.Values,
            IsSuccess = valuesResult.IsSuccess,
            FailReason = valuesResult.FailReason,
        };
        return result;
    }
}

public class Harness_ : BinanceDataTest
{
    [Fact]
    public async void _()
    {
        var h = new BufferingIndicatorHarness<TIndicator, TParameters, IKline, decimal>(ServiceProvider, new()
        {
            Parameters = new TParameters
            {
                MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                Period = 14,
            },
            TimeFrame = TimeFrame.h1,
            InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        });

        var result = await h.GetReverseValues(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Values);
        Assert.Equal(5, result.Values.Count);

        // 2024.04.01 h1 14 Simple - verified to 0.1 via TradingView
        Assert.Equal(466.84285714285714285714285714M, result.Values[4]);
        Assert.Equal(529.78571428571428571428571429M, result.Values[3]);
        Assert.Equal(562.53571428571428571428571429M, result.Values[2]);
        Assert.Equal(605.25714285714285714285714286M, result.Values[1]);
        Assert.Equal(619.87142857142857142857142857M, result.Values[0]);
    }
}

public class Indicator_from_MarketData : BinanceDataTest
{
    [Fact]
    public async void _()
    {
        var historicalTimeSeries = ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve<decimal>(
            new IndicatorHarnessOptions<TParameters>()
            {
                Parameters = new TParameters
                {
                    Period = 14,
                    MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                },
                TimeFrame = TimeFrame.h1,
                InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
            });


        //var h = new BufferingIndicatorHarness<TIndicator, TParameters, IKline, decimal>();

        var result = await historicalTimeSeries.Get(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
    new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Items);
        Assert.Equal(5, result.Items.Length);

        // 2024.04.01 h1 14 Simple - verified to 0.1 via TradingView
        Assert.Equal(466.84285714285714285714285714M, result.Items[4]);
        Assert.Equal(529.78571428571428571428571429M, result.Items[3]);
        Assert.Equal(562.53571428571428571428571429M, result.Items[2]);
        Assert.Equal(605.25714285714285714285714286M, result.Items[1]);
        Assert.Equal(619.87142857142857142857142857M, result.Items[0]);
    }
}
