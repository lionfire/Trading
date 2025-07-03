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
    //: IBarListener
        where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    public IAccount2<TPrecision> Account { get; }
    public ExchangeSymbol ExchangeSymbol { get; set; }

    #endregion

    #region DEPRECATED

    static BotInfo BotInfo => BotInfos.Get(typeof(PSimAccount<TPrecision>), typeof(SimAccount<TPrecision>));

    //List<InputMapping> IHasInputMappings.InputMappings
    //=>        [];
    //=> [new(PMultiSim.Bars!, BotInfo.InputParameterToValueMapping![0])];

    //IBarListener IHasInputMappings.Instance => this;


    #endregion

    #region Lifecycle

    public AccountMarketSim(IAccount2<TPrecision> account, ExchangeSymbol exchangeSymbol)
    {
        Account = account;
        ExchangeSymbol = exchangeSymbol;
    }

    public void Init(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException("TODO: Init Bars");
    }

    #endregion

    #region Inputs

    #region Market Data


    // ENH: OHLC
    public IReadOnlyValuesWindow<HLC<TPrecision>> Bars { get; set; } = null!;

    #endregion 

    //IReadOnlyList<SignalInfo> IHasSignalInfo.GetSignalInfos() => signalInfos ??= [new SignalInfo(typeof(SimAccount<TPrecision>).GetProperty(nameof(Bars))!)];
    //IReadOnlyList<SignalInfo> signalInfos;

    #endregion

}
