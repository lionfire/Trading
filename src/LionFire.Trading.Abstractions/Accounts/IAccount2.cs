using System.Data.SqlTypes;
using System.Numerics;

namespace LionFire.Trading;


public enum MarketFeatures
{
    Unspecified = 0,
    Exists = 1 << 0,
    Long = 1 << 1,
    Short = 1 << 2,
    Ticks = 1 << 3,
    Bars = 1 << 4,
    OrderBook = 1 << 5,
    //OrderBookDepth = 1 << 6,
    //OrderBookL2 = 1 << 8,

}

public interface IAccount2 : IAccount2<double>, IAccount2<decimal>
{
    #region Identity

    string Exchange { get; }
    string ExchangeArea { get; }

    bool IsSimulation { get; }
    bool IsLive => !IsSimulation;

    bool IsRealMoney { get; }

    /// <summary>
    /// If true, supports multiple long and short positions for the same symbol
    /// </summary>
    bool IsHedging { get; }

    #endregion

    MarketFeatures GetMarketFeatures(string symbol);


    IEnumerable<IPosition> Positions { get; }
}

public interface IAccount2<TPrecision>
    where TPrecision : INumber<TPrecision>
{
    ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize);

    IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
}

public interface IOrderResult
{
    bool? IsSuccess { get; }
    bool IsComplete { get; }
    string Error { get; }
    // TODO

}
