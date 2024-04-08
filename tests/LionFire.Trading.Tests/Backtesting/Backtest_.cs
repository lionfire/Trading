

//namespace LionFire.Trading.Indicators.Harnesses.Tests;

using LionFire.Trading.Backtesting;

namespace Backtesting_;

public class Backtest_ : BinanceDataTest
{
    [Fact]
    public async void _()
    {
        //var h = new HistoricalIndicatorHarness<TIndicator, TParameters, IKline, decimal>(ServiceProvider, new()
        //{
        //    Parameters = new TParameters
        //    {
        //        //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        //        MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
        //        Period = 14
        //    },
        //    TimeFrame = TimeFrame.h1,
        //    InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        //});

    }
}

public class BacktestTask2
{
    #region Input

    #endregion

    #region Output

    #endregion

    #region Parameters

    public SymbolBarsRange Range { get; set; }

    #endregion

    #region State

    public BacktestAccount BacktestAccount { get; private set; }
    public DateTimeOffset BacktestDate { get; set; } = DateTimeOffset.UtcNow;

    #endregion
}