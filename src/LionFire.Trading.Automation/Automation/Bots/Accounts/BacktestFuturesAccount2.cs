using DynamicData;
using System.Numerics;

namespace LionFire.Trading.Automation;

public class BacktestFuturesAccount2<TPrecision> : SimulatedAccount2<TPrecision>
    where TPrecision : INumber<TPrecision>
{

    #region Accounts

    public BacktestBotController BacktestBotController { get; }

    #endregion

    #region Lifecycle

    public BacktestFuturesAccount2(BacktestBotController backtestBotController, string exchange, string exchangeArea) : base(exchange, exchangeArea)
    {
        BacktestBotController = backtestBotController;
    }

    #endregion

    #region Methods

    int positionIdCounter = 0;


    public override ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, double positionSize)
    {
        var p = new PositionBase()
        {
            Id = positionIdCounter++,
            //EntryTime = ,            
            EntryPrice = 0,
            Quantity = (decimal)positionSize,
            SymbolId = new SymbolId { Symbol = symbol },
            TakeProfit = null,
            StopLoss = null,
        };
        positions.AddOrUpdate(p);
        return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = p });
    }

    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, double positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }
    public override ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) { throw new NotImplementedException(); }
    public override ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, decimal positionSize) { throw new NotImplementedException(); }
    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }

    #endregion

}
