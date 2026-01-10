using DynamicData;
using System.Reactive.Linq;

namespace LionFire.Trading.Automation;

public class PSimulatedHoldingPrecisionAdapter<TPrecision, TFromPrecision> : PMarketProcessor<TPrecision>, IPSimulatedHolding<TPrecision>
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

    public TFromPrecision Convert(TPrecision to) => TFromPrecision.CreateChecked(to);
    public TPrecision ConvertFrom(TFromPrecision from) => TPrecision.CreateChecked(from);
}

public class AccountPrecisionAdapter<TPrecision, TFromPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
    where TFromPrecision : struct, INumber<TFromPrecision>
{

    public IAccount2<TFromPrecision> From { get; }

    IPMarketProcessor? IMarketListener.Parameters => Parameters as IPMarketProcessor;

    public IPSimulatedHolding<TPrecision>? Parameters => PAccountPrecisionAdapter;
    private PSimulatedHoldingPrecisionAdapter<TPrecision, TFromPrecision>? PAccountPrecisionAdapter;
    IPHolding IAccount2.PPrimaryHolding => From.PPrimaryHolding;

    private readonly IObservableCache<IPosition<TPrecision>, int> _positionsCache;

    public AccountPrecisionAdapter(IAccount2<TFromPrecision> from)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        var fromHolding = from.PPrimaryHolding as IPSimulatedHolding<TFromPrecision>;
        if (fromHolding != null)
        {
            PAccountPrecisionAdapter = new PSimulatedHoldingPrecisionAdapter<TPrecision, TFromPrecision>(fromHolding)
            {
                ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame("DEFAULT_EXCHANGE", "DEFAULT_AREA", fromHolding.Symbol, TimeFrame.m1)
            };
        }
        // else: PAccountPrecisionAdapter remains null - live accounts don't have PPrimaryHolding

        // Transform positions from source precision to target precision
        _positionsCache = From.Positions
            .Connect()
            .Transform(p => (IPosition<TPrecision>)new PositionPrecisionAdapter<TPrecision, TFromPrecision>(p, this))
            .AsObservableCache();
    }

    public IObservableCache<IPosition<TPrecision>, int> Positions => _positionsCache;


    public ExchangeArea ExchangeArea => From.ExchangeArea;


    public bool IsSimulation => From.IsSimulation;

    public bool IsRealMoney => From.IsRealMoney;

    public bool IsHedging => From.IsHedging;

    public TPrecision Balance => ConvertFrom(From.Balance);

    public IObservableCache<IHolding<TPrecision>, string> Holdings => throw new NotImplementedException();

    public ISimHolding<TPrecision> PrimaryHolding => throw new NotImplementedException();

    public float ListenOrder { get => From.ListenOrder;  }


    public ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified) => From.ClosePosition(From.Positions.Lookup(position.Id).Value);

    // Use INumber<T>.CreateChecked for proper numeric conversion between types like decimal and double
    public TFromPrecision Convert(TPrecision from) => TFromPrecision.CreateChecked(from);
    public TPrecision ConvertFrom(TFromPrecision from) => TPrecision.CreateChecked(from);

    public IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null)
    {
        return From.ClosePositionsForSymbol(symbol, longAndShort, Convert(positionSize), postOnly, marketExecuteAtPrice, stopLimit);
    }

    public ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified) => From.ExecuteMarketOrder(symbol, longAndShort, Convert(positionSize), increasePositionFlags, existingPositionId, transactionId: transactionId, journalFlags: journalFlags);

    public MarketFeatures GetMarketFeatures(string symbol) => From.GetMarketFeatures(symbol);

    public void OnBar() => From.OnBar();

    public ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) => From.ReducePositionForSymbol(symbol, longAndShort, positionSize);

    public void OnRealizedProfit(TPrecision realizedGrossProfitDelta) => From.OnRealizedProfit(Convert(realizedGrossProfitDelta));

    public ValueTask<IOrderResult> SetTakeProfits(string symbol, LongAndShort direction, TPrecision tp, StopLossFlags tightenOnly)
        => From.SetTakeProfits(symbol, direction, Convert(tp), tightenOnly);

    public ValueTask<IOrderResult> SetStopLosses(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags tightenOnly)
        => From.SetStopLosses(symbol, direction, Convert(sl), tightenOnly);
}

/// <summary>
/// Adapts a position from one numeric precision type to another.
/// </summary>
public class PositionPrecisionAdapter<TPrecision, TFromPrecision> : IPosition<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
    where TFromPrecision : struct, INumber<TFromPrecision>
{
    private readonly IPosition<TFromPrecision> _from;
    private readonly AccountPrecisionAdapter<TPrecision, TFromPrecision> _account;

    public PositionPrecisionAdapter(IPosition<TFromPrecision> from, AccountPrecisionAdapter<TPrecision, TFromPrecision> account)
    {
        _from = from ?? throw new ArgumentNullException(nameof(from));
        _account = account ?? throw new ArgumentNullException(nameof(account));
    }

    private TPrecision Convert(TFromPrecision value) => _account.ConvertFrom(value);
    private TFromPrecision ConvertBack(TPrecision value) => _account.Convert(value);
    private TPrecision? ConvertNullable(TFromPrecision? value) => value.HasValue ? Convert(value.Value) : null;
    private TFromPrecision? ConvertBackNullable(TPrecision? value) => value.HasValue ? ConvertBack(value.Value) : null;

    public int Id => _from.Id;
    public string Symbol => _from.Symbol;
    public SymbolId SymbolId => _from.SymbolId;
    public LongAndShort LongOrShort => _from.LongOrShort;
    public TradeKind TradeType => _from.TradeType;
    public TPrecision Quantity => Convert(_from.Quantity);
    public long Volume => _from.Volume;
    public TPrecision EntryAverage => Convert(_from.EntryAverage);
    public DateTimeOffset EntryTime => _from.EntryTime;
    public TPrecision? LastPrice { get => ConvertNullable(_from.LastPrice); set => _from.LastPrice = ConvertBackNullable(value); }
    public TPrecision? LiqPrice { get => ConvertNullable(_from.LiqPrice); set => _from.LiqPrice = ConvertBackNullable(value); }
    public TPrecision? MarkPrice { get => ConvertNullable(_from.MarkPrice); set => _from.MarkPrice = ConvertBackNullable(value); }
    public TPrecision? UsdEquivalentQuantity { get => ConvertNullable(_from.UsdEquivalentQuantity); set => _from.UsdEquivalentQuantity = ConvertBackNullable(value); }
    public TPrecision GrossProfit => Convert(_from.GrossProfit);
    public TPrecision RealizedGrossProfit => Convert(_from.RealizedGrossProfit);
    public TPrecision Commissions => Convert(_from.Commissions);
    public TPrecision Swap => Convert(_from.Swap);
    public TPrecision NetProfit => Convert(_from.NetProfit);
    public TPrecision Pips => Convert(_from.Pips);
    public TPrecision? StopLoss => ConvertNullable(_from.StopLoss);
    public TPrecision? TakeProfit => ConvertNullable(_from.TakeProfit);
    public string? StopLossWorkingType { get => _from.StopLossWorkingType; set => _from.StopLossWorkingType = value; }
    public string? Label => _from.Label;
    public string? Comment => _from.Comment;
    public IAccount2<TPrecision> Account => _account;

    public void Close() => _from.Close();

    public ValueTask<IOrderResult> SetStopLoss(TPrecision price) => _from.SetStopLoss(_account.Convert(price));
    public ValueTask<IOrderResult> SetTakeProfit(TPrecision price) => _from.SetTakeProfit(_account.Convert(price));
}
