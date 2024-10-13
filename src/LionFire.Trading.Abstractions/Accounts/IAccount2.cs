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

public interface IPSimulatedAccount2<TPrecision> : IPAccount2
    where TPrecision : struct, INumber<TPrecision>
{
    TPrecision StartingBalance { get; set; }
    //DateTimeOffset StartTime { get; set; }


    TPrecision AbortOnBalanceDrawdownPerunum { get; set; }
}

public interface ISimulatedAccount2<TPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    ValueTask<IOrderResult> SimulatedExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, TPrecision? currentPrice = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified);
    TPrecision InitialBalance { get; }
    TPrecision BalanceReturnOnInvestment { get; }
    double AnnualizedBalanceReturnOnInvestment { get; }
    double AnnualizedBalanceReturnOnInvestmentVsDrawdownPercent { get; }
    double AnnualizedReturnOnInvestmentVsDrawdownPercent { get; }

        TPrecision MaxEquityDrawdownPercent { get; }
    TPrecision CurrentEquityDrawdown { get; }
    TPrecision MaxEquityDrawdown { get; }

    TPrecision MaxBalanceDrawdownPerunum { get; }
    TPrecision CurrentBalanceDrawdown { get; }
    TPrecision MaxBalanceDrawdown { get; }
    bool IsAborted { get; }
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

}

public interface IAccount2<TPrecision> : IAccount2
    where TPrecision : struct, INumber<TPrecision>
{
    new IPSimulatedAccount2<TPrecision> Parameters { get; }

    #region Balance

    TPrecision Balance { get; }

    // FUTURE: Multiple balances

    #endregion

    ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified);

    ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified);
    IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    IObservableCache<IPosition<TPrecision>, int> Positions { get; }

    ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
    void OnRealizedProfit(TPrecision realizedGrossProfitDelta);
    ValueTask<IOrderResult> SetStopLosses(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags flags);
    ValueTask<IOrderResult> SetTakeProfits(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags flags);
}


public enum StopLossFlags
{
    Unspecified = 0,
    TightenOnly = 1 << 0,
}

[Flags]
public enum JournalEntryFlags
{
    Unspecified = 0,
    StopLoss = 1 << 0,
    TakeProfit = 1 << 1,
    Reverse = 1 << 10,
}
