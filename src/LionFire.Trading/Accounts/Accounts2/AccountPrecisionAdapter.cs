using DynamicData;

namespace LionFire.Trading;

public class PAccountPrecisionAdapter<TPrecision, TFromPrecision> : IPAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
    where TFromPrecision : struct, INumber<TFromPrecision>
{
    public IPAccount2<TFromPrecision> From { get; }

    public string? BalanceCurrency => From.BalanceCurrency;

    public TPrecision StartingBalance { get => ConvertFrom(From.StartingBalance); set => From.StartingBalance = Convert(value); }

    public PAccountPrecisionAdapter(IPAccount2<TFromPrecision> parameters)
    {
        From = parameters;
    }

    public TFromPrecision Convert(TPrecision to) => (TFromPrecision)(object)to;
    public TPrecision ConvertFrom(TFromPrecision from) => (TPrecision)(object)from;
}

public class AccountPrecisionAdapter<TPrecision, TFromPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
    where TFromPrecision : struct, INumber<TFromPrecision>
{
    public IAccount2<TFromPrecision> From { get; }

    public IPAccount2<TPrecision> Parameters => PAccountPrecisionAdapter;
    private PAccountPrecisionAdapter<TPrecision, TFromPrecision> PAccountPrecisionAdapter;
    IPAccount2 IAccount2.Parameters => From.Parameters;

    public AccountPrecisionAdapter(IAccount2<TFromPrecision> from)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        PAccountPrecisionAdapter = new PAccountPrecisionAdapter<TPrecision, TFromPrecision>(from.Parameters);
    }

    public IObservableCache<IPosition, int> Positions => throw new NotImplementedException();


    public string Exchange => From.Exchange;

    public string ExchangeArea => From.ExchangeArea;

    public bool IsSimulation => From.IsSimulation;

    public bool IsRealMoney => From.IsRealMoney;

    public bool IsHedging => From.IsHedging;

    public TPrecision Balance => ConvertFrom(From.Balance);

    public ValueTask<IOrderResult> ClosePosition(IPosition position) => From.ClosePosition(position);

    //public TPrecision Convert(TFromPrecision from) => (TPrecision)(object)from;
    public TFromPrecision Convert(TPrecision from) => (TFromPrecision)(object)from;
    public TPrecision ConvertFrom(TFromPrecision from) => (TPrecision)(object)from;

    public IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null)
    {
        return From.ClosePositionsForSymbol(symbol, longAndShort, Convert(positionSize), postOnly, marketExecuteAtPrice, stopLimit);
    }

    public ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null) => From.ExecuteMarketOrder(symbol, longAndShort, Convert(positionSize), increasePositionFlags, existingPositionId);

    public MarketFeatures GetMarketFeatures(string symbol) => From.GetMarketFeatures(symbol);

    public void OnBar() => From.OnBar();

    public ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) => From.ReducePositionForSymbol(symbol, longAndShort, positionSize);

    public void OnRealizedProfit(TPrecision realizedGrossProfitDelta) => From.OnRealizedProfit(Convert(realizedGrossProfitDelta));
}
