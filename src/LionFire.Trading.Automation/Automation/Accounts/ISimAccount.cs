using System.Numerics;

namespace LionFire.Trading.Automation;

public interface ISimAccount<TPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Parameters

    new IPSimulatedHolding<TPrecision>? PPrimaryHolding { get; }

    //TPrecision InitialBalance { get; }

    #endregion

    #region Methods

    ValueTask<IOrderResult> SimulatedExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, TPrecision? currentPrice = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified);
    
    IEnumerable<AccountMarketSim<TPrecision>> GetAllMarketSims();
    
    #endregion


    bool IsAborted { get; }
    DateTimeOffset? AbortDate { get; }

    long GetNextTransactionId();
}
