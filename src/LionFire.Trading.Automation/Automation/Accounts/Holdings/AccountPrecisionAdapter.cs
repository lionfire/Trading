using DynamicData;

namespace LionFire.Trading.Automation;

public class PSimulatedHoldingPrecisionAdapter<TPrecision, TFromPrecision> : IPSimulatedHolding<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
    where TFromPrecision : struct, INumber<TFromPrecision>
{
    #region Identity

    public string Symbol => From.Symbol;

    #endregion

    #region Relationships

    public IPSimulatedHolding<TFromPrecision> From { get; }

    #endregion

    #region Lifecycle

    public PSimulatedHoldingPrecisionAdapter(IPSimulatedHolding<TFromPrecision> parameters)
    {
        From = parameters;
    }

    #endregion

    public TPrecision StartingBalance { get => ConvertFrom(From.StartingBalance); set => From.StartingBalance = Convert(value); }
    public PAssetProtection<TPrecision>? AssetProtection
    {
        get => throw new NotImplementedException(); // From.AssetProtection;
        set => throw new NotImplementedException(); //From.AssetProtection = value;
    }

    public TFromPrecision Convert(TPrecision to) => (TFromPrecision)(object)to;
    public TPrecision ConvertFrom(TFromPrecision from) => (TPrecision)(object)from;
}

public class AccountPrecisionAdapter<TPrecision, TFromPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
    where TFromPrecision : struct, INumber<TFromPrecision>
{

    public IAccount2<TFromPrecision> From { get; }

    public IPSimulatedHolding<TPrecision> Parameters => PAccountPrecisionAdapter;
    private PSimulatedHoldingPrecisionAdapter<TPrecision, TFromPrecision> PAccountPrecisionAdapter;
    IPHolding IAccount2.PPrimaryHolding => From.PPrimaryHolding;

    public AccountPrecisionAdapter(IAccount2<TFromPrecision> from)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        PAccountPrecisionAdapter = new PSimulatedHoldingPrecisionAdapter<TPrecision, TFromPrecision>((IPSimulatedHolding<TFromPrecision>)from.PPrimaryHolding);
    }

    public IObservableCache<IPosition<TPrecision>, int> Positions => throw new NotImplementedException();


    public ExchangeArea ExchangeArea => From.ExchangeArea;


    public bool IsSimulation => From.IsSimulation;

    public bool IsRealMoney => From.IsRealMoney;

    public bool IsHedging => From.IsHedging;

    public TPrecision Balance => ConvertFrom(From.Balance);

    public IObservableCache<IHolding<TPrecision>, string> Holdings => throw new NotImplementedException();

    public ISimHolding<TPrecision> PrimaryHolding => throw new NotImplementedException();

    public float ListenOrder { get => From.ListenOrder;  }

    public ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified) => From.ClosePosition(From.Positions.Lookup(position.Id).Value);

    //public TPrecision Convert(TFromPrecision from) => (TPrecision)(object)from;
    public TFromPrecision Convert(TPrecision from) => (TFromPrecision)(object)from;
    public TPrecision ConvertFrom(TFromPrecision from) => (TPrecision)(object)from;

    public IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null)
    {
        return From.ClosePositionsForSymbol(symbol, longAndShort, Convert(positionSize), postOnly, marketExecuteAtPrice, stopLimit);
    }

    public ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified) => From.ExecuteMarketOrder(symbol, longAndShort, Convert(positionSize), increasePositionFlags, existingPositionId, transactionId: transactionId, journalFlags: journalFlags);

    public MarketFeatures GetMarketFeatures(string symbol) => From.GetMarketFeatures(symbol);

    public void OnBar() => From.OnBar();

    public ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) => From.ReducePositionForSymbol(symbol, longAndShort, positionSize);

    public void OnRealizedProfit(TPrecision realizedGrossProfitDelta) => From.OnRealizedProfit(Convert(realizedGrossProfitDelta));

    public ValueTask<IOrderResult> SetTakeProfits(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags tightenOnly)
    {
        throw new NotImplementedException();
    }
    public ValueTask<IOrderResult> SetStopLosses(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags tightenOnly)
    {
        throw new NotImplementedException();
    }
}
