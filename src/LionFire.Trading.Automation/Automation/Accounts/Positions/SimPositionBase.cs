using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;


public sealed class SimPosition<TPrecision> : PositionBaseBase<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{

    public AccountMarketSim<TPrecision> AccountMarketSim { get; private set; }

    #region Relationships

    public override string Symbol => AccountMarketSim.ExchangeSymbol.Symbol;
    public override IAccount2<TPrecision> Account => AccountMarketSim.Account;

    #endregion

    #region Lifecycle

    public SimPosition(AccountMarketSim<TPrecision> accountMarketSim) 
    {
        AccountMarketSim = accountMarketSim;
    }

    #endregion

    public override ValueTask<IOrderResult> SetStopLoss(TPrecision price)
    {
        StopLoss = price;
        return ValueTask.FromResult<IOrderResult>(OrderResult.Success);
    }
    public override ValueTask<IOrderResult> SetTakeProfit(TPrecision price)
    {
        TakeProfit = price;
        return ValueTask.FromResult<IOrderResult>(OrderResult.Success);
    }

}

