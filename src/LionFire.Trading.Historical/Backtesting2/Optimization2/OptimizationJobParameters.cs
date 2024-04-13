#nullable enable
using LionFire.Trading.Algos.Modular.Filters;
using LionFire.Trading.Backtesting2;

namespace LionFire.Trading.Optimizing2;


#if OLD // TOTRIAGE
public abstract class SignalProvider : IValueProvider<double>
{
    public BacktestRunner? Context { get; set; }

    //public SignalProvider(BacktestRunner backtestRunner)
    //{
    //    Context = backtestRunner;
    //}

    public abstract double this[int barsAgo] { get; }
}


//public class RsiFilter : SignalProvider, IFilterProvider
//{
//    public double SellIfAbove { get; set; } = 80.0;
//    public double BuyIfBelow { get; set; } = 20.0;

//    public bool? IsCounterTrend => SellIfAbove > 50 && BuyIfBelow < 50;

//    DataSeries<double>? DataSeries { get; set; }

//    public override double this[int barsAgo]
//    {
//        get
//        {
//            for ()
//            {

//            }
//        }
//    }
//}

public class MovingAverageParameters
{
    public MovingAverageType Kind { get; set; }

    public int Period { get; set; }

}

public class MAFilter : SignalProvider, IFilterProvider
{
    public TradeKind? IfAbove { get; set; } = TradeKind.Buy;
    public TradeKind? IfBelow { get; set; } = TradeKind.Sell;

    public MovingAverageParameters MovingAverageParameters { get; set; }

    DataSeries<double>? DataSeries { get; set; }

    //QuantConnect.Indicators.ExponentialMovingAverage

    public override double this[int barsAgo]
    {
        get
        {
            //for (int i )
            //    DataSeries[];

            //for ()
            //{
            throw new NotImplementedException();
            //}
        }
    }
}

#endif

public class OptimizationJobParameters
{
    public List<string> AlgoTypes { get; set; } = new List<string>();

    public List<string> ExchangesWithAreas { get; set; } = new List<string>();

    public List<string> SymbolsToTrade { get; set; } = new List<string>();
    public List<string> SymbolsToWatch { get; set; } = new List<string>();


    #region Constraints

    public bool AbortFailures { get; set; } = true;

    #endregion


    // public GeneticOptimizationOptions? GeneticOptions {get;set;}

    // public bool? Deterministic { get;set; }

}
