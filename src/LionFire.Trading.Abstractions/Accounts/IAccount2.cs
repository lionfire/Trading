using DynamicData;
using LionFire.Trading.Automation;

namespace LionFire.Trading;

/// <summary>
/// An account for a user (or sub-user) within a single exchange (e.g. Binance) and area (e.g. spot or futures)
/// </summary>
public interface IAccount2 : IBarListener
{
    #region Identity

    public ExchangeArea ExchangeArea { get; }

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

    #region Parameters

    IPHolding? PPrimaryHolding { get; }

    #endregion

    MarketFeatures GetMarketFeatures(string symbol);
}

/// <summary>
/// An account for a user (or sub-user) within a single exchange (e.g. Binance) and area (e.g. spot or futures)
/// </summary>
public interface IAccount2<TPrecision> : IAccount2
    where TPrecision : struct, INumber<TPrecision>
{
    #region State

    IObservableCache<IHolding<TPrecision>, string /* Symbol  */> Holdings { get; }

    #region (Convenience) Balance

    ISimHolding<TPrecision>? PrimaryHolding { get; }

    TPrecision Balance { get; }

    #endregion

    #endregion

    #region Methods

    ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified);

    ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified);
    IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    IObservableCache<IPosition<TPrecision>, int> Positions { get; }

    ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
    void OnRealizedProfit(TPrecision realizedGrossProfitDelta);
    ValueTask<IOrderResult> SetStopLosses(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags flags);
    ValueTask<IOrderResult> SetTakeProfits(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags flags);

    #endregion

}
