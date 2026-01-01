using System.Collections.Concurrent;
using System.Numerics;
using DynamicData;
using LionFire.Trading.DataFlow;

namespace LionFire.Trading.Automation.Accounts;

/// <summary>
/// Base class for live trading accounts, providing common functionality for
/// <see cref="LiveSimAccount{TPrecision}"/> and <see cref="LivePaperAccount{TPrecision}"/>.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type used for calculations.</typeparam>
public abstract class LiveAccountBase<TPrecision> : ILiveAccount<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Configuration

    /// <summary>
    /// Gets the options used to configure this account.
    /// </summary>
    public LiveAccountOptions Options { get; }

    /// <summary>
    /// Gets the exchange area for this account.
    /// </summary>
    public ExchangeArea ExchangeArea { get; }

    #endregion

    #region ILiveAccount Implementation

    /// <inheritdoc />
    public abstract BotAccountMode Mode { get; }

    /// <inheritdoc />
    public abstract TPrecision Balance { get; }

    /// <inheritdoc />
    public abstract TPrecision Equity { get; }

    /// <inheritdoc />
    public TPrecision UnrealizedPnL => CalculateUnrealizedPnL();

    /// <inheritdoc />
    public TPrecision RealizedPnL { get; protected set; }

    /// <inheritdoc />
    public TPrecision InitialBalance { get; }

    /// <inheritdoc />
    public string BaseCurrency => Options.BaseCurrency;

    #endregion

    #region IAccount2 Implementation

    /// <inheritdoc />
    public bool IsSimulation => true;

    /// <inheritdoc />
    public bool IsRealMoney => false;

    /// <inheritdoc />
    public bool IsHedging => true;

    /// <inheritdoc />
    public HedgingKind HedgingKind => HedgingKind.Hedging;

    /// <inheritdoc />
    public IPHolding? PPrimaryHolding => null;

    /// <inheritdoc />
    public ISimHolding<TPrecision>? PrimaryHolding => null;

    /// <inheritdoc />
    public IObservableCache<IHolding<TPrecision>, string> Holdings => throw new NotImplementedException("Holdings not yet implemented for live accounts");

    /// <inheritdoc />
    public IObservableCache<IPosition<TPrecision>, int> Positions => _positionsCache;

    #endregion

    #region IMarketListener / IBarListener Implementation

    /// <inheritdoc />
    public float ListenOrder => ListenerOrders.Account;

    /// <inheritdoc />
    /// <remarks>
    /// Returns null as live accounts don't require market processor parameters
    /// for live trading scenarios.
    /// </remarks>
    IPMarketProcessor IMarketListener.Parameters => _marketParameters;
    private readonly LiveAccountMarketParameters _marketParameters;

    /// <inheritdoc />
    /// <remarks>
    /// Called when a bar completes in live trading. Updates position prices
    /// based on the latest market data.
    /// </remarks>
    public void OnBar()
    {
        // In live trading, position prices are updated via price monitoring,
        // not via bar events. This is a no-op for live accounts.
    }

    #endregion

    #region Position Tracking

    private readonly ConcurrentDictionary<int, LivePosition<TPrecision>> _positions = new();
    private readonly SourceCache<IPosition<TPrecision>, int> _positionsSource;
    private readonly IObservableCache<IPosition<TPrecision>, int> _positionsCache;
    private int _nextPositionId = 1;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of the live account.
    /// </summary>
    /// <param name="exchangeArea">The exchange and area for this account.</param>
    /// <param name="options">The account configuration options.</param>
    protected LiveAccountBase(ExchangeArea exchangeArea, LiveAccountOptions options)
    {
        ExchangeArea = exchangeArea;
        Options = options ?? throw new ArgumentNullException(nameof(options));
        InitialBalance = TPrecision.CreateChecked(options.InitialBalance);
        RealizedPnL = TPrecision.Zero;

        _positionsSource = new SourceCache<IPosition<TPrecision>, int>(p => p.Id);
        _positionsCache = _positionsSource.AsObservableCache();
        _marketParameters = new LiveAccountMarketParameters(exchangeArea);
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Called when a position is closed to update account state.
    /// </summary>
    /// <param name="pnl">The realized P&amp;L from the closed position.</param>
    protected abstract void OnPositionClosed(TPrecision pnl);

    #endregion

    #region Position Management

    /// <summary>
    /// Opens a new position.
    /// </summary>
    public virtual LivePosition<TPrecision> OpenPosition(
        string symbol,
        LongAndShort direction,
        TPrecision size,
        TPrecision entryPrice)
    {
        var position = new LivePosition<TPrecision>
        {
            Id = Interlocked.Increment(ref _nextPositionId),
            Symbol = symbol,
            Direction = direction,
            Size = size,
            EntryPrice = entryPrice,
            EntryTime = DateTimeOffset.UtcNow,
            CurrentPrice = entryPrice,
            Account = this
        };

        _positions[position.Id] = position;
        _positionsSource.AddOrUpdate(position);

        return position;
    }

    /// <summary>
    /// Closes a position and realizes the P&amp;L.
    /// </summary>
    public virtual TPrecision ClosePosition(int positionId, TPrecision exitPrice)
    {
        if (!_positions.TryRemove(positionId, out var position))
        {
            throw new InvalidOperationException($"Position {positionId} not found");
        }

        position.ExitPrice = exitPrice;
        position.ExitTime = DateTimeOffset.UtcNow;
        position.IsClosed = true;

        var pnl = CalculatePositionPnL(position, exitPrice);
        RealizedPnL += pnl;

        OnPositionClosed(pnl);

        _positionsSource.Remove(positionId);

        return pnl;
    }

    /// <summary>
    /// Updates the current price for a position (for unrealized P&amp;L calculation).
    /// </summary>
    public virtual void UpdatePositionPrice(int positionId, TPrecision currentPrice)
    {
        if (_positions.TryGetValue(positionId, out var position))
        {
            position.CurrentPrice = currentPrice;
        }
    }

    #endregion

    #region P&L Calculation

    /// <summary>
    /// Calculates the P&amp;L for a position at a given price.
    /// </summary>
    protected virtual TPrecision CalculatePositionPnL(LivePosition<TPrecision> position, TPrecision price)
    {
        var priceDiff = price - position.EntryPrice;

        // For short positions, P&L is inverted
        if (position.Direction == LongAndShort.Short)
        {
            priceDiff = -priceDiff;
        }

        return priceDiff * position.Size;
    }

    /// <summary>
    /// Calculates total unrealized P&amp;L across all open positions.
    /// </summary>
    protected virtual TPrecision CalculateUnrealizedPnL()
    {
        var total = TPrecision.Zero;

        foreach (var position in _positions.Values)
        {
            total += CalculatePositionPnL(position, position.CurrentPrice);
        }

        return total;
    }

    #endregion

    #region IAccount2 Methods (Stubs for now)

    /// <inheritdoc />
    public MarketFeatures GetMarketFeatures(string symbol)
    {
        // Return default market features for now
        return new MarketFeatures();
    }

    /// <inheritdoc />
    public virtual ValueTask<IOrderResult> ExecuteMarketOrder(
        string symbol,
        LongAndShort longAndShort,
        TPrecision positionSize,
        PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default,
        int? existingPositionId = null,
        long? transactionId = null,
        JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified)
    {
        // This will be implemented when we have price monitoring
        throw new NotImplementedException("ExecuteMarketOrder requires price monitor integration");
    }

    /// <inheritdoc />
    public virtual ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified)
    {
        // This will be implemented when we have price monitoring
        throw new NotImplementedException("ClosePosition requires price monitor integration");
    }

    /// <inheritdoc />
    public IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(
        string symbol,
        LongAndShort longAndShort,
        TPrecision positionSize,
        bool postOnly = false,
        decimal? marketExecuteAtPrice = null,
        (decimal? stop, decimal? limit)? stopLimit = null)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void OnRealizedProfit(TPrecision realizedGrossProfitDelta)
    {
        RealizedPnL += realizedGrossProfitDelta;
    }

    /// <inheritdoc />
    public ValueTask<IOrderResult> SetStopLosses(string symbol, LongAndShort direction, TPrecision sl, StopLossFlags flags)
    {
        // This will be implemented with pending order manager
        throw new NotImplementedException("SetStopLosses requires pending order manager integration");
    }

    /// <inheritdoc />
    public ValueTask<IOrderResult> SetTakeProfits(string symbol, LongAndShort direction, TPrecision tp, StopLossFlags flags)
    {
        // This will be implemented with pending order manager
        throw new NotImplementedException("SetTakeProfits requires pending order manager integration");
    }

    #endregion
}

/// <summary>
/// Represents an open position in a live account.
/// </summary>
public class LivePosition<TPrecision> : IPosition<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    /// <inheritdoc />
    public int Id { get; init; }

    /// <inheritdoc />
    public string Symbol { get; init; } = string.Empty;

    /// <inheritdoc />
    public SymbolId SymbolId => new(Symbol);

    /// <summary>
    /// Gets or sets an optional label for this position.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets an optional comment for this position.
    /// </summary>
    public string? Comment { get; set; }

    #endregion

    #region Direction and Size

    /// <summary>
    /// Gets the direction of this position (Long or Short).
    /// </summary>
    public LongAndShort Direction { get; init; }

    /// <inheritdoc />
    public LongAndShort LongOrShort => Direction;

    /// <inheritdoc />
    public TradeKind TradeType => Direction == LongAndShort.Long ? TradeKind.Buy : TradeKind.Sell;

    /// <summary>
    /// Gets or sets the position size.
    /// </summary>
    public TPrecision Size { get; set; }

    /// <inheritdoc />
    public TPrecision Quantity => Size;

    /// <inheritdoc />
    public long Volume => long.CreateChecked(Size);

    #endregion

    #region Entry

    /// <summary>
    /// Gets the entry price for this position.
    /// </summary>
    public TPrecision EntryPrice { get; init; }

    /// <inheritdoc />
    public TPrecision EntryAverage => EntryPrice;

    /// <inheritdoc />
    public DateTimeOffset EntryTime { get; init; }

    #endregion

    #region Current Price and P&L

    /// <summary>
    /// Gets or sets the current market price for unrealized P&amp;L calculation.
    /// </summary>
    public TPrecision CurrentPrice { get; set; }

    /// <inheritdoc />
    public TPrecision? LastPrice { get; set; }

    /// <inheritdoc />
    public TPrecision? LiqPrice { get; set; }

    /// <inheritdoc />
    public TPrecision? MarkPrice { get; set; }

    /// <inheritdoc />
    public TPrecision? UsdEquivalentQuantity { get; set; }

    /// <summary>
    /// Gets the unrealized gross profit (price difference * quantity, adjusted for direction).
    /// </summary>
    public TPrecision GrossProfit
    {
        get
        {
            var priceDiff = CurrentPrice - EntryPrice;
            if (Direction == LongAndShort.Short)
            {
                priceDiff = -priceDiff;
            }
            return priceDiff * Size;
        }
    }

    /// <inheritdoc />
    public TPrecision RealizedGrossProfit { get; set; }

    /// <inheritdoc />
    public TPrecision Commissions { get; set; }

    /// <inheritdoc />
    public TPrecision Swap { get; set; }

    /// <summary>
    /// Gets the net profit (gross profit minus commissions and swap).
    /// </summary>
    public TPrecision NetProfit => GrossProfit - Commissions - Swap;

    /// <summary>
    /// Gets the profit in pips.
    /// </summary>
    /// <remarks>
    /// For simplified simulation, this returns the price difference.
    /// Real pip calculation requires symbol pip size information.
    /// </remarks>
    public TPrecision Pips
    {
        get
        {
            var priceDiff = CurrentPrice - EntryPrice;
            if (Direction == LongAndShort.Short)
            {
                priceDiff = -priceDiff;
            }
            return priceDiff;
        }
    }

    #endregion

    #region Exit

    /// <summary>
    /// Gets or sets the exit price when the position is closed.
    /// </summary>
    public TPrecision? ExitPrice { get; set; }

    /// <summary>
    /// Gets or sets the exit time when the position is closed.
    /// </summary>
    public DateTimeOffset? ExitTime { get; set; }

    /// <summary>
    /// Gets or sets whether this position is closed.
    /// </summary>
    public bool IsClosed { get; set; }

    #endregion

    #region Stop Loss / Take Profit

    /// <inheritdoc />
    public TPrecision? StopLoss { get; set; }

    /// <inheritdoc />
    public TPrecision? TakeProfit { get; set; }

    /// <inheritdoc />
    public string? StopLossWorkingType { get; set; }

    /// <summary>
    /// Closes this position.
    /// </summary>
    /// <remarks>
    /// This method marks the position as closed. The actual P&amp;L realization
    /// should be handled by the account that manages this position.
    /// </remarks>
    public void Close()
    {
        if (!IsClosed)
        {
            IsClosed = true;
            ExitTime = DateTimeOffset.UtcNow;
            ExitPrice = CurrentPrice;
        }
    }

    /// <summary>
    /// Sets the stop loss price for this position.
    /// </summary>
    /// <param name="price">The stop loss trigger price.</param>
    /// <returns>The order result (always succeeds for simulated positions).</returns>
    public ValueTask<IOrderResult> SetStopLoss(TPrecision price)
    {
        StopLoss = price;
        return ValueTask.FromResult<IOrderResult>(OrderResult.Success);
    }

    /// <summary>
    /// Sets the take profit price for this position.
    /// </summary>
    /// <param name="price">The take profit trigger price.</param>
    /// <returns>The order result (always succeeds for simulated positions).</returns>
    public ValueTask<IOrderResult> SetTakeProfit(TPrecision price)
    {
        TakeProfit = price;
        return ValueTask.FromResult<IOrderResult>(OrderResult.Success);
    }

    #endregion

    #region Account Reference

    /// <summary>
    /// Gets or sets the account that owns this position.
    /// </summary>
    public IAccount2<TPrecision> Account { get; set; } = null!;

    #endregion
}

/// <summary>
/// Market processor parameters for live accounts.
/// </summary>
/// <remarks>
/// Live accounts don't require the same bar-based input system as backtesting accounts.
/// This is a minimal implementation to satisfy the IMarketListener interface.
/// </remarks>
internal class LiveAccountMarketParameters : IPMarketProcessor
{
    private readonly ExchangeArea _exchangeArea;

    public LiveAccountMarketParameters(ExchangeArea exchangeArea)
    {
        _exchangeArea = exchangeArea;
    }

    /// <inheritdoc />
    public int[]? InputLookbacks => null;

    /// <inheritdoc />
    /// <remarks>
    /// Returns a placeholder input for live accounts. In live trading,
    /// price data comes from real-time feeds rather than bar inputs.
    /// </remarks>
    public IPInput Bars => _placeholderInput;
    private static readonly LiveAccountBarsInput _placeholderInput = new();
}

/// <summary>
/// Placeholder input for live account market parameters.
/// </summary>
internal class LiveAccountBarsInput : IPInput
{
    public string Key => "live-bars";
    public Type? ValueType => null;
}
