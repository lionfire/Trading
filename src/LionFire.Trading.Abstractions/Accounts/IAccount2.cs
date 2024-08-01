using System.Numerics;

namespace LionFire.Trading;

public interface IMarketParticipant2
{

    void OnBar();

}

public interface IPAccount2 { }
public interface IAccount2 : IAccount2<double>, IAccount2<decimal>
{
    IPAccount2 Parameters { get; }

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

    ValueTask<IOrderResult> ClosePosition(IPosition position);
}

public interface IAccount2<TPrecision> : IMarketParticipant2
    where TPrecision : INumber<TPrecision>
{
    ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize);

    IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);


    ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
}

