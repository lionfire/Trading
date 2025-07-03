using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

// REFACTOR: Merge into BatchHarness
public class Sim<TPrecision>
        where TPrecision : struct, INumber<TPrecision>
{
    #region Markets

    public MarketSim<TPrecision>? DefaultMarket { get; set; }

    public MarketSim<TPrecision>? GetMarket(ExchangeSymbol exchangeSymbol)
    {
        if (DefaultMarket != null && DefaultMarket.ExchangeSymbol == exchangeSymbol) return DefaultMarket;

        if (markets != null && markets.TryGetValue(exchangeSymbol, out var market)) return market;

        lock (marketsLock)
        {
            markets ??= new Dictionary<ExchangeSymbol, MarketSim<TPrecision>>();

            if (!markets.TryGetValue(exchangeSymbol, out market))
            {
                market = new MarketSim<TPrecision>(exchangeSymbol);
                markets[exchangeSymbol] = market;
            }

            return market;
        }
    }

    Dictionary<ExchangeSymbol, MarketSim<TPrecision>>? markets;
    private readonly object marketsLock = new object();

    #endregion

    #region Inputs

    List<IInputEnumerator> Inputs { get; } = new();

    #endregion

    #region Listeners

    /// <summary>
    /// Uses DefaultTimeFrame
    /// </summary>
    public SortedList<float, IBarListener> DefaultBarListeners { get; } = new();

    // TODO FUTURE: multiple timeframe support
    //public Dictionary<DefaultTimeFrame, SortedList<float, IBarListener>> BarListeners { get; } = new();

    #endregion

    #region Methods

    public ValueTask Advance()
    {
        throw new NotImplementedException();
        foreach (var x in Inputs)
        {
            // TODO: Populate with data - where is the existing code for this?  See MultiBacktestHarness
        }

        foreach (var x in DefaultBarListeners.Values)
        {
            x.OnBar();
        }
    }

    #endregion

}
