#if NEXT
//namespace LionFire.Trading.Indicators.Harnesses.Tests;

using Backtesting_;
using LionFire.Collections;
using LionFire.Instantiating;
using LionFire.Structures;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Bots;
using LionFire.Trading.Indicators;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.QuantConnect_;
using Microsoft.Extensions.DependencyInjection;

namespace Backtesting_;

public class Backtest_ : BinanceDataTest
{
    [Fact]
    public async void _()
    {

        var backtestTask = new BacktestTask2<PAtrBot>(ServiceProvider, new PAtrBot
        {
            ATR = new PAverageTrueRange
            {
                MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                Period = 8,
            }
        })
        {
        };

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

// Activating bots
// - Try ctor(TParameters)
// - Try ctor(IServiceProvider, TParameters)
// - Try GetService<IFactory<TBot, TParameters>>().Create(TParameters)
// - save successful result to static
public class BacktestTask2<TParameters, TBot>
    where TBot : IBot2
{
    //TBot Bot { get; }

    public BacktestTask2(IServiceProvider serviceProvider, TParameters parameters)
    {
        if (typeof(TParameters) is IFactory<TBot> factory)
        {
            //Bot = factory.Create()
        }

    }
}
public class PBot2
{
}

public interface IBot2
{

}

public class BotBase2<TParameters> : IBot2
{
    public TParameters Parameters => parameters;
    private PBot2 parameters;

    public BotBase2(PBot2 parameters)
    {
        this.parameters = parameters;
    }

}
/// <summary>
/// Long if ATR has increased for N straight bars
/// </summary>
public class Bot2<TParameters> : BotBase2<TParameters>
    where TParameters : PBot2
{
    public Bot2(IServiceProvider serviceProvider, TParameters parameters) : base(parameters)
    {
    }
}



// TODO
//public class LiveTradingContext : ITradingContext
//{
//    bool IsLive => true;
//}

public class PAtrBot : PBot2
{
    public required PAverageTrueRange ATR { get; init; }
}

//public class AtrBot : Bot2<PAtrBot>
//{
//    public AtrBot()
//    {
//    }
//}

public class PBacktestTask2<PBot, TBot>
     where PBot : ITemplate<TBot>
{
    public bool TicksEnabled { get; set; }

    /// <summary>
    /// Refuse to run if order book info is not available for the symbol being traded
    /// </summary>
    public bool OrderBook { get; set; }

    public required SymbolBarsRange SymbolBarsRange { get; init; }

    /// <summary>
    /// Primary symbol being traded.
    /// (Some bots may trade multiple symbols, in which case this symbol may be unused or superfluous.)
    /// </summary>
    public string Symbol => SymbolBarsRange.Symbol;
    public TimeFrame TimeFrame => SymbolBarsRange.TimeFrame;
    public DateTimeOffset Start => SymbolBarsRange.Start;
    public DateTimeOffset EndExclusive => SymbolBarsRange.EndExclusive;
}

public class BotHarness
{

}

public abstract class SimulatedAccount
{

}

public class BacktestAccount2 : SimulatedAccount
{

    #region State

    public double Balance { get; set; }

    #endregion

}


public class HistoricalBotHarness
{
    public HistoricalBotHarness(IBot bot)
    {

    }
}



public class BacktestTask2
// : IProgress<double>  // ENH
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Input

    #endregion

    #region Output

    #endregion

    #region Parameters

    public PBacktestTask2 Parameters { get; }

    #endregion

    #region Lifecycle

    public BacktestTask2(IServiceProvider serviceProvider, PBacktestTask2 parameters)
    {
        ServiceProvider = serviceProvider;
        Parameters = parameters;
    }

    #endregion

    #region State

    public BacktestAccount BacktestAccount { get; private set; }
    public DateTimeOffset BacktestDate { get; set; } = DateTimeOffset.UtcNow;

    #endregion

    #region Methods

    public Task Start(CancellationToken cancellationToken = default)
    {
        BacktestDate = Parameters.Start;

    }

    public void NextBar()
    {

    }

    #endregion
}
#endif