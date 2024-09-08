using DynamicData;
using System.Numerics;

namespace LionFire.Trading;

public interface IMarketParticipant2
{

    void OnBar();

}

public interface IPAccount2
{
    string? BalanceCurrency { get; }

}
public interface IPAccount2<TPrecision> : IPAccount2
    where TPrecision : struct, INumber<TPrecision>
{
    TPrecision StartingBalance { get; set; }
}

public interface IAccount2 : IMarketParticipant2
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

    // TODO: Duplicate of IsHedging
    HedgingKind HedgingKind => HedgingKind.Hedging;

    #endregion


    MarketFeatures GetMarketFeatures(string symbol);

    ValueTask<IOrderResult> ClosePosition(IPosition position);
}

public interface IAccount2<TPrecision> : IAccount2
    where TPrecision : struct, INumber<TPrecision>
{
    new IPAccount2<TPrecision> Parameters { get; }

    #region Balance

    TPrecision Balance { get; }

    // FUTURE: Multiple balances

    #endregion

    ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null);

    IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    //IReadOnlyList<IPosition<TPrecision>> Positions { get; }
    IObservableCache<IPosition, int> Positions { get; }

    ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
    void OnRealizedProfit(TPrecision realizedGrossProfitDelta);
}


