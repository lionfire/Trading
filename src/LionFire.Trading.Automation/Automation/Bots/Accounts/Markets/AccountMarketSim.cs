using LionFire.Trading.Automation.Bots;
using LionFire.Trading.ValueWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TPrecision"></typeparam>
/// <remarks>
/// Limitations: Only supports HLC Bars, no ticks.  ENH: Support more options somehow, perhaps via other classes/generic class
/// </remarks>
public class AccountMarketSim<TPrecision>
    : IMarketListener, IHasInputMappings
        where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    public IAccount2<TPrecision> Account { get; }
    public ExchangeSymbol ExchangeSymbol { get; set; }
    
    /// <summary>
    /// Optional context for this market sim. When set, provides lifecycle management and coordination.
    /// </summary>
    public AccountMarketSimContext<TPrecision>? Context { get; set; }

    #endregion

    #region IMarketListener

    public float ListenOrder => ListenerOrders.AccountMarket;

    public IPMarketProcessor Parameters { get; }

    #endregion

    #region IHasInputMappings

    public List<PInputToMappingToValuesWindowProperty>? InputMappings { get; set; }

    #endregion

    #region Lifecycle

    public AccountMarketSim(IAccount2<TPrecision> account, ExchangeSymbol exchangeSymbol)
    {
        Account = account;
        ExchangeSymbol = exchangeSymbol;

        Parameters = new PAccountMarketSim<TPrecision>
        {
            ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(ExchangeSymbol.Exchange, ExchangeSymbol.Area, ExchangeSymbol.Symbol, ((SimAccount<TPrecision>)Account).Context.TimeFrame)
        };

        // TODO: Optimize: Make this a static readonly?
        InputMappings = new List<PInputToMappingToValuesWindowProperty>
        {
            new PInputToMappingToValuesWindowProperty(
                Parameters.Bars, 
                new InputParameterToValueMapping(
                    typeof(PAccountMarketSim<TPrecision>).GetProperty(nameof(PAccountMarketSim<TPrecision>.Bars))!,
                    typeof(AccountMarketSim<TPrecision>).GetProperty(nameof(Bars))!
                ))
        };
    }

    public void Init(IServiceProvider serviceProvider)
    {
        // Initialization will be handled by the input mapping system
    }

    #endregion

    #region Inputs

    #region Market Data


    // ENH: OHLC
    [Signal(0)]
    public IReadOnlyValuesWindow<HLC<TPrecision>> Bars { get; set; } = null!;

    #endregion 

    //IReadOnlyList<SignalInfo> IHasSignalInfo.GetSignalInfos() => signalInfos ??= [new SignalInfo(typeof(SimAccount<TPrecision>).GetProperty(nameof(Bars))!)];
    //IReadOnlyList<SignalInfo> signalInfos;

    #endregion

}
