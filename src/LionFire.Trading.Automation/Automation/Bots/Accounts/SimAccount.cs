using DynamicData;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Journal;
using System.Diagnostics;

namespace LionFire.Trading.Automation;

//public sealed class AccountSimEx<TPrecision>
//    : SimAccount<TPrecision>
//    , IHasSignalInfo
//    , IHasInputMappings // this is here so the account can liquidate positions if they lose too much, and also calculate equity.  REVIEW: move to common account class?
//    , IBarListener
//    , IMarketListenerOrder
//    where TPrecision : struct, INumber<TPrecision>
//{
//}

/// <summary>
/// Account for a user (or sub-user) within a single exchange (e.g. Binance) and area (e.g. spot or futures)
/// </summary>
/// <typeparam name="TPrecision"></typeparam>
public sealed class SimAccount<TPrecision> : ISimAccount<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Dependencies

    public BotContext<TPrecision> Context { get; }
    public SimContext<TPrecision> SimContext => Context.Sim;

    //private BacktestsJournal Journal => SimContext.MultiSimContext.Journal;
    private IBotTradeJournal<TPrecision> BotJournal => Context.BotJournal;

    #endregion

    #region Identity

    #region Derived

    public ExchangeArea ExchangeArea => Parameters.ExchangeArea;

    #endregion

    #region REVIEW

    public bool IsSimulation => true;

    public bool IsRealMoney => false;

    public bool IsHedging => false;

    #endregion

    #endregion

    #region Parameters

    public float ListenOrder => ListenerOrders.Account;

    public PSimAccount<TPrecision> Parameters { get; }

    #region Convenience

    public IPSimulatedHolding<TPrecision>? PPrimaryHolding => Parameters.DefaultHolding;
    IPHolding? IAccount2.PPrimaryHolding => Parameters.DefaultHolding;

    #endregion

    #endregion

    #region Lifecycle

    internal SimAccount(BotContext<TPrecision> context, PSimAccount<TPrecision> parameters)
    {
        Context = context;
        Parameters = parameters;

        if (PPrimaryHolding != null)
        {
            PrimaryHolding = new SimHolding<TPrecision>(this, PPrimaryHolding);
        }

        if (SimContext.PMultiSim.DefaultSymbol != null)
        {
            DefaultMarketSim = CreateMarketSim(SimContext.PMultiSim.DefaultSymbol);
        }
    }

    #endregion

    public MarketFeatures GetMarketFeatures(string symbol)
    {
        throw new NotImplementedException();
    }

    #region State

    #region Holdings

    public SimHolding<TPrecision>? PrimaryHolding { get; }
    ISimHolding<TPrecision>? IAccount2<TPrecision>.PrimaryHolding => PrimaryHolding;

    public IObservableCache<IHolding<TPrecision>, string /* DefaultSymbol  */> Holdings => holdings;
    private SourceCache<IHolding<TPrecision>, string> holdings { get; } = new(h => h.Symbol);
    private readonly object holdingsLock = new();

    public IHolding<TPrecision> GetHolding(string symbol)
    {
        if (symbol == PrimaryHolding?.Symbol) { return PrimaryHolding; }

        ArgumentNullException.ThrowIfNull(symbol);

        var optional = holdings.Lookup(symbol);

        if (optional.HasValue) return optional.Value;

        lock (holdingsLock)
        {
            optional = holdings.Lookup(symbol);
            if (optional.HasValue) return optional.Value;
            else
            {
                var p = new PSimulatedHolding<TPrecision>() { Symbol = symbol };
                var result = new SimHolding<TPrecision>(this, p);
                holdings.AddOrUpdate(result);
                //InputMappingTools.HydrateInputMappings(inputEnumerators, account);
                return result;
            }
        }
    }

    #endregion

    #region Markets

    public AccountMarketSim<TPrecision>? DefaultMarketSim { get; }

    public AccountMarketSim<TPrecision> GetMarketSim(string symbol)
    {
        if (symbol == DefaultMarketSim?.ExchangeSymbol.Symbol) { return DefaultMarketSim; }

        if (marketSims.TryGetValue(symbol, out var result)) return result;

        lock (marketSimsLock)
        {
            if (marketSims.TryGetValue(symbol, out result)) return result;

            result = CreateMarketSim(symbol);
            marketSims.Add(symbol, result);
            return result;
        }
    }
    private readonly Dictionary<string, AccountMarketSim<TPrecision>> marketSims = [];
    private readonly object marketSimsLock = new();

    private AccountMarketSim<TPrecision> CreateMarketSim(string symbol)
    {
        return new AccountMarketSim<TPrecision>(this, new ExchangeSymbol(ExchangeArea, symbol));
    }

    #endregion

    public TPrecision Balance
    {
        get => PrimaryHolding == null ? TPrecision.Zero : PrimaryHolding.Balance;
        set => (PrimaryHolding ?? throw new NotSupportedException($"{nameof(PrimaryHolding)} is null")).Balance = value;
    }

    public BotAbortReason AbortReason { get; set; }
    internal void Abort(BotAbortReason abortReason)
    {
        AbortReason = abortReason;
        AbortDate = SimContext.SimulatedCurrentDate;
        //End = SimContext.SimulatedCurrentDate; // OLD
        OnSimAccountAborted(this);
    }

    #region Time

    //public DateTimeOffset Start { get; set; }
    //public DateTimeOffset End { get; set; }
    public bool IsAborted => AbortDate.HasValue;
    public DateTimeOffset? AbortDate { get; private set; }

    public DateTimeOffset DateTime => SimContext.SimulatedCurrentDate;

    #endregion

    #region Positions

    int positionIdCounter = 0;

    public const bool CanPositionChangeDirections = false;

    public IObservableCache<IPosition<TPrecision>, int> Positions => positions;
    protected SourceCache<IPosition<TPrecision>, int> positions = new(p => p.Id);

    #endregion

    #region Orders

    public long GetNextTransactionId() => nextTransactionId++;
    private long nextTransactionId = 1;

    #endregion

    #endregion

    #region FUTURE: Account Journal?

    //public AccountState<TPrecision> State => stateJournal.Value;
    // How to do state? Event sourcing, or snapshots?

    //public IObservable<AccountState<TPrecision>> StateJournal => stateJournal;
    //private BehaviorSubject<AccountState<TPrecision>> stateJournal = new(AccountState<TPrecision>.Uninitialized);

    //private void OnStateChanging()
    //{
    //    if (!stateJournal.Value.IsInitialized || stateJournal.Value!.Time != State.Time)
    //    {
    //        //stateJournal.OnNext(new AccountState<TPrecision> { States = new List<(DateTimeOffset time, AccountState<TPrecision> state)> { (State.Time, State) } });
    //    }
    //}
    #endregion

    #region Event Handlers

    int barCount = 0;
    public async void OnBar()
    {
        await ProcessStopLossAndTakeProfit();

        #region Log: aliveness

        if (barCount++ <= 0)
        {
            if (typeof(TPrecision) == typeof(double))
            {
                //double trigger = 100.0;
                //double trigger = 68;
                //var BarsCasted = (IReadOnlyValuesWindow<HLC<double>, double>)Bars;
                //BarsCasted.SubscribeToPrice(trigger, (HLC<double> bar) =>
                //{
                //    Debug.WriteLine("----------------- Subscription triggered. Trigger: {0}, Bar: {1}", trigger, bar);
                //}, PriceSubscriptionDirection.Up);
            }
        }
        if (barCount % 25000 == 0)
        {
            //Debug.WriteLine($"Bar #{barCount} {Bars[0]}, lookback: {Bars.Size}");
            Debug.WriteLine($"Bar #{barCount}");
        }

        #endregion

    }

    public void OnRealizedProfit(TPrecision realizedGrossProfitDelta)
    {
        Balance += realizedGrossProfitDelta;
    }

    private void OnSimAccountAborted(ISimAccount<TPrecision> simulatedAccount)
    {
        BotJournal.Write(new JournalEntry<TPrecision>(simulatedAccount)
        {
            EntryType = JournalEntryType.Abort,
            Time = SimContext.SimulatedCurrentDate,
        });
    }

    #endregion

    #region Methods

    #region Methods: Market execution

    public ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified) => SimulatedExecuteMarketOrder(symbol, longAndShort, positionSize, increasePositionFlags, existingPositionId, transactionId, journalFlags: journalFlags);

    // ENH Idea: break this up into atomic operations. E.g.:
    // - reduce position 1 to 0 (close)
    // - open new position in the amount of x.
    // - transaction:
    //   - transaction Id
    //   - aggregate result
    //   - ENH: all or nothing support
    public async ValueTask<IOrderResult> SimulatedExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision requestedQuantityChange, PositionOperationFlags operationFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, TPrecision? currentPrice = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified)
    {
        if (requestedQuantityChange == TPrecision.Zero) { return new OrderResult { IsSuccess = true, Noop = true }; }
        if (longAndShort == LongAndShort.Unspecified || longAndShort == LongAndShort.LongAndShort)
        {
            if (operationFlags.HasFlag(PositionOperationFlags.Open) && !operationFlags.HasFlag(PositionOperationFlags.CloseOnly))
            {
                if (requestedQuantityChange > TPrecision.Zero) { longAndShort = LongAndShort.Long; }
                else if (requestedQuantityChange < TPrecision.Zero) { longAndShort = LongAndShort.Short; }
            }
            if (longAndShort == LongAndShort.Unspecified && operationFlags.HasFlag(PositionOperationFlags.Close))
            {
                if (requestedQuantityChange > TPrecision.Zero) { longAndShort = LongAndShort.Short; }
                else if (requestedQuantityChange < TPrecision.Zero) { longAndShort = LongAndShort.Long; }
            }
        }

        bool isIncrease;
        bool allowCloseAndOpenAtOnce = (operationFlags & PositionOperationFlags.AllowCloseAndOpenAtOnce) == PositionOperationFlags.AllowCloseAndOpenAtOnce;

        switch (longAndShort)
        {
            case LongAndShort.Long:
                isIncrease = requestedQuantityChange > TPrecision.Zero;
                break;
            case LongAndShort.Short:
                isIncrease = requestedQuantityChange < TPrecision.Zero;
                break;
            case LongAndShort.Unspecified:
            case LongAndShort.LongAndShort:
            default:
                throw new ArgumentException($"Invalid {nameof(longAndShort)}: {longAndShort}");
        }

        transactionId ??= GetNextTransactionId();
        currentPrice ??= CurrentPrice(symbol);
        var requestedQuantityChangeRemaining = requestedQuantityChange;
        List<IOrderResult>? innerResults = null;

        if ((operationFlags & PositionOperationFlags.ResizeExistingPosition) == PositionOperationFlags.ResizeExistingPosition)
        {
            TPrecision remainingSize = requestedQuantityChange;

            //if (!isIncrease) // Decrease
            {

                #region Try to find position in requested long/short direction, and Increase or Decrease/close that position

                bool didSomething = false;
            tryAgain:
                bool madeProgressThisTurn = false;
                // Try to find existing position
                foreach (var position in positions.KeyValues.Select(kvp => kvp.Value).OfType<SimPosition<TPrecision>>()
                    .Where(p => p.Symbol == symbol
                                   && p.LongOrShort == longAndShort))
                {
                    TPrecision quantityDelta;

                    if (isIncrease)
                    {
                        quantityDelta = requestedQuantityChangeRemaining;
                    }
                    else
                    {
                        quantityDelta = TPrecision.Max(TPrecision.Abs(requestedQuantityChange), TPrecision.Abs(position.Quantity));
                        quantityDelta = TPrecision.CopySign(quantityDelta, requestedQuantityChange);
                    }

                    using var _ = (new PositionModification(position));

                    madeProgressThisTurn = true;
                    didSomething = true;

                    var oldQuantity = position.Quantity;
                    var oldEntryAverage = position.EntryAverage;
                    var entryEquity = position.Quantity * position.EntryAverage;
                    var currentEquity = position.Quantity * currentPrice;

                    position.Quantity += quantityDelta;

                    TPrecision realizedGrossProfitDelta = TPrecision.Zero;
                    if (!isIncrease)
                    {
                        realizedGrossProfitDelta = (position.EntryAverage - currentPrice.Value) * quantityDelta;
                        position.RealizedGrossProfit += realizedGrossProfitDelta;
                        position.Account.OnRealizedProfit(realizedGrossProfitDelta);
                    }

                    requestedQuantityChangeRemaining -= quantityDelta;

                    if (position.Quantity == TPrecision.Zero)
                    {
                        position.EntryAverage = currentPrice.Value;

                        if (CanPositionChangeDirections && requestedQuantityChangeRemaining != TPrecision.Zero)
                        {
                            // Position is reversing directions

                            journalFlags |= JournalEntryFlags.Reverse;
                            position.ResetDirection();
                            position.Quantity = requestedQuantityChangeRemaining;
                            quantityDelta += requestedQuantityChangeRemaining;
                            //requestedQuantityChangeRemaining = TPrecision.Zero; // Unneeded

                            BotJournal.Write(new JournalEntry<TPrecision>(position)
                            {
                                TransactionId = transactionId,
                                Time = DateTime,
                                EntryType = JournalEntryType.Modify,
                                Flags = journalFlags,
                                QuantityChange = quantityDelta,
                                Price = currentPrice,
                                RealizedGrossProfitDelta = realizedGrossProfitDelta,
                            });
                            throw new NotImplementedException("UNTESTED"); // Not sure if this branch is needed/desired, or correct yet
                        }
                        else
                        {
                            // Position is zero size and should be closed 
                            OnClosingPosition(position, journalFlags, currentPrice, quantityDelta, realizedGrossProfitDelta, transactionId.Value);
                        }
                    }
                    else
                    {
                        // Position still has some quantity remaining and has not changed directions.

                        // Update the new EntryAverage
                        position.EntryAverage = ((oldQuantity * oldEntryAverage + quantityDelta * currentPrice.Value) / position.Quantity);

                        BotJournal.Write(new JournalEntry<TPrecision>(position)
                        {
                            TransactionId = transactionId,
                            Time = DateTime,
                            EntryType = JournalEntryType.Modify,
                            Flags = journalFlags,
                            Symbol = symbol,
                            QuantityChange = quantityDelta,
                            Price = currentPrice,
                            RealizedGrossProfitDelta = realizedGrossProfitDelta,
                        });
                    }

                    var result = new OrderResult { IsSuccess = true, Data = position };
                    if (requestedQuantityChangeRemaining == TPrecision.Zero) { return result; }
                    else { (innerResults ??= new()).Add(result); }
                }

                if (madeProgressThisTurn && !isIncrease && requestedQuantityChangeRemaining != TPrecision.Zero && positions.Count > 0)
                {
                    // UNTESTED
                    madeProgressThisTurn = false;
                    goto tryAgain; // Try to decrease other positions from the same symbol until the requested amount is closed
                }

                #endregion

                #region If PositionOperationFlags.AllowCloseAndOpenAtOnce, open position in opposite direction

                if (didSomething && operationFlags.HasFlag(PositionOperationFlags.AllowCloseAndOpenAtOnce) && requestedQuantityChangeRemaining > TPrecision.Zero)
                {
                    // Open/increase new position in opposite direction
                    // TODO FIXME: Result should probably be an aggregate of the close in one direction and the open/increase in the other direction.
                    return await ExecuteMarketOrder(symbol, longAndShort.Opposite(), requestedQuantityChangeRemaining, operationFlags, transactionId: transactionId);
                    //innerResults ??= [];
                    //innerResults.Add(innerResult);
                    //return new OrderResult { IsSuccess = true, Data = position, InnerResults = innerResults };
                }

                #endregion
            }
        }

        if (requestedQuantityChangeRemaining != TPrecision.Zero)
        {
            if (!operationFlags.HasFlag(PositionOperationFlags.Open) || operationFlags.HasFlag(PositionOperationFlags.CloseOnly))
            {
                //Debug.WriteLine($"Not opening position of size {requestedQuantityChange} because of PositionOperationFlags.");
                return innerResults != null && innerResults.Count > 0
                    ? new OrderResult
                    {
                        IsSuccess = true,
                        InnerResults = innerResults
                    }
                    : new OrderResult { IsSuccess = true, Noop = true };
            }
            else
            {
                //bool increase = requestedQuantityChangeRemaining > TPrecision.Zero
                //    ? longAndShort == LongAndShort.Long
                //    : longAndShort == LongAndShort.Short -- broken;

                var p = new SimPosition<TPrecision>(GetMarketSim(symbol))
                {
                    Id = positionIdCounter++,
                    EntryAverage = currentPrice.Value,
                    Quantity = requestedQuantityChangeRemaining,
                    TakeProfit = default,
                    StopLoss = default,
                    EntryTime = DateTime,
                };
                positions.AddOrUpdate(p);

                BotJournal.Write(new JournalEntry<TPrecision>(p)
                {
                    TransactionId = transactionId,
                    Time = DateTime,
                    EntryType = JournalEntryType.Open,
                    Flags = journalFlags,
                    QuantityChange = p.Quantity,
                    Price = p.EntryAverage,
                });//.FireAndForget();

                return new OrderResult { IsSuccess = true, Data = p, InnerResults = innerResults };
            }
        }
        throw new UnreachableCodeException();
    }

    #region Close

    public IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }

    public ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified) => ClosePosition((SimPosition<TPrecision>)position, flags, currentPrice: null);


    public ValueTask<IOrderResult> ClosePosition(SimPosition<TPrecision> position, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified, TPrecision? currentPrice = null) // TODO: Execution options: PostOnly, etc.
    {
        currentPrice ??= CurrentPrice(position.Symbol);

        TPrecision realizedGrossProfitDelta = position.ProfitAtPrice(currentPrice.Value);
        position.RealizedGrossProfit += realizedGrossProfitDelta;
        position.Account.OnRealizedProfit(realizedGrossProfitDelta);

        var oldQuantity = position.Quantity;
        position.Quantity = TPrecision.Zero;

        OnClosingPosition(position, journalFlags, currentPrice, -oldQuantity, realizedGrossProfitDelta, GetNextTransactionId());

        return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = position }); // TODO: ClosePositionResult, with PnL
    }

    public async ValueTask<IOrderResult> SetStopLosses(string symbol, LongAndShort direction, TPrecision triggerPrice, StopLossFlags flags)
    {
        #region Result

        IOrderResult? result = null;
        List<IOrderResult>? inner = null;
        bool allSuccess = true;

        void OnResult(IOrderResult r)
        {
            if (!r.IsSuccess) { allSuccess = false; }
            if (result == null) { result = r; }
            else
            {
                inner ??= new List<IOrderResult> { result };
                inner.Add(r);
            }
        }

        #endregion

        foreach (var position in Positions.Items.Where(p => p.Symbol == symbol).OfType<IPosition<TPrecision>>())
        {
            if (flags.HasFlag(StopLossFlags.TightenOnly))
            {
                switch (position.LongOrShort)
                {
                    case LongAndShort.Long:
                        if (position.StopLoss >= triggerPrice)
                        {
                            OnResult(new OrderResult { IsSuccess = true, Data = position, Noop = true });
                            continue;
                        }
                        break;
                    case LongAndShort.Short:
                        if (position.StopLoss <= triggerPrice)
                        {
                            OnResult(new OrderResult { IsSuccess = true, Data = position, Noop = true });
                            continue;
                        }
                        break;
                    default:
                        throw new UnreachableCodeException();
                }
            }
            await position.SetStopLoss(triggerPrice);
            OnResult(new OrderResult { IsSuccess = true, Data = position });
        }

        return inner != null
            ? new OrderResult { IsSuccess = allSuccess, InnerResults = inner }
            : result ?? new OrderResult { IsSuccess = true, Noop = true };

    }

    #endregion

    #endregion


    protected TPrecision CurrentPrice(string symbol)
    {
        var market = GetMarketSim(symbol);
        return market.Bars[0].Close;
    }

    protected void OnClosingPosition(SimPosition<TPrecision> position, JournalEntryFlags journalFlags, TPrecision? currentPrice, TPrecision quantityChange, TPrecision realizedGrossProfitDelta, long transactionId) // TODO: Execution options: PostOnly, etc.
    {
        BotJournal.Write(new JournalEntry<TPrecision>(position)
        {
            TransactionId = transactionId,
            Time = DateTime,
            EntryType = JournalEntryType.Close,
            Flags = journalFlags,
            QuantityChange = quantityChange,
            RealizedGrossProfitDelta = realizedGrossProfitDelta,
            Price = currentPrice,
        });

        positions.Remove(position);
    }

    public ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) { throw new NotImplementedException(); }

    #endregion

    #region Stop Loss

    public async ValueTask<IOrderResult> SetTakeProfits(string symbol, LongAndShort direction, TPrecision triggerPrice, StopLossFlags flags)
    {
        #region Result

        IOrderResult? result = null;
        List<IOrderResult>? inner = null;
        bool allSuccess = true;

        void OnResult(IOrderResult r)
        {
            if (!r.IsSuccess) { allSuccess = false; }
            if (result == null) { result = r; }
            else
            {
                inner ??= new List<IOrderResult> { result };
                inner.Add(r);
            }
        }

        #endregion

        foreach (var position in Positions.Items.Where(p => p.Symbol == symbol).OfType<IPosition<TPrecision>>())
        {
            if (flags.HasFlag(StopLossFlags.TightenOnly))
            {
                switch (position.LongOrShort)
                {
                    case LongAndShort.Long:
                        if (position.TakeProfit <= triggerPrice)
                        {
                            OnResult(new OrderResult { IsSuccess = true, Data = position, Noop = true });
                            continue;
                        }
                        break;
                    case LongAndShort.Short:
                        if (position.TakeProfit >= triggerPrice)
                        {
                            OnResult(new OrderResult { IsSuccess = true, Data = position, Noop = true });
                            continue;
                        }
                        break;
                    default:
                        throw new UnreachableCodeException();
                }
            }
            await position.SetTakeProfit(triggerPrice);
            OnResult(new OrderResult { IsSuccess = true, Data = position });
        }

        return inner != null
            ? new OrderResult { IsSuccess = allSuccess, InnerResults = inner }
            : result ?? new OrderResult { IsSuccess = true, Noop = true };

    }


    #endregion

    #region Simulation // MOVE

    private async ValueTask ProcessStopLossAndTakeProfit()
    {
        foreach (var position in Positions.Items.OfType<SimPosition<TPrecision>>())
        {
            // FUTURE:
            //position.ProcessStopLossAndTakeProfit();
            //PrimaryHolding.Bars

            if ((position.StopLoss != default &&
                       (
                        (position.LongOrShort == LongAndShort.Long && position.AccountMarketSim.Bars[0].Low <= position.StopLoss)
                        || (position.LongOrShort == LongAndShort.Short && position.AccountMarketSim.Bars[0].High >= position.StopLoss)
                       )
                   )
              )
            {
                await ClosePosition(position, JournalEntryFlags.StopLoss, currentPrice: position.StopLoss.Value);
            }
            if ((position.TakeProfit != default &&
                     (
                      (position.LongOrShort == LongAndShort.Long && position.AccountMarketSim.Bars[0].High >= position.TakeProfit)
                      || (position.LongOrShort == LongAndShort.Short && position.AccountMarketSim.Bars[0].Low <= position.TakeProfit)
                     )
                 )
              )
            {
                await ClosePosition(position, JournalEntryFlags.TakeProfit, currentPrice: position.TakeProfit.Value);
            }
        }
    }

    #endregion

}

// FUTURE, maybe, or else use composition to build logic for live accounts
// - e.g. monitor simulated account vs real money account, alert on discrepancies
//public class LiveAccount2<TPrecision>
//    : Account2Base<TPrecision>
//    , IHasSignalInfo
//    , IHasInputMappings // this is here so the account can liquidate positions if they lose too much, and also calculate equity.  REVIEW: move to common account class?
//    where TPrecision : struct, INumber<TPrecision>
//{
//    public LiveAccount2(PBacktestAccount<TPrecision> parameters, BotBatchBacktestContext<TPrecision> controller, string exchange, string exchangeArea, string? symbol = null) : base(parameters, controller, exchange, exchangeArea, symbol)
//    {
//    }
//}
//public class BacktestAccount2<TPrecision>
//    : SimulatedAccount2<TPrecision>
//    , IHasSignalInfo
//    , IHasInputMappings // this is here so the account can liquidate positions if they lose too much, and also calculate equity.  REVIEW: move to common account class?
//    where TPrecision : struct, INumber<TPrecision>
//{
//    public SimulatedAccount2(PBacktestAccount<TPrecision> parameters, BotBatchBacktestContext<TPrecision> controller, string exchange, string exchangeArea, string? symbol = null) : base(parameters, controller, exchange, exchangeArea, symbol)
//    {
//    }
//}

#if FUTURE // maybe

public class AccountStateJournal<TPrecision_Inner>
        where TPrecision : struct, INumber<TPrecision>
    {
        public List<(DateTimeOffset time, AccountState<TPrecision> state)> States { get; set; } = new();
    }

    public readonly struct AccountState<TCurrency>
        where TCurrency : INumber<TCurrency>
    {
        public static readonly AccountState<TCurrency> Uninitialized = new()
        {
            Balance = TCurrency.Zero,
            Equity = TCurrency.Zero,
            Time = default,
        };

        public AccountState()
        {
        }

        public readonly DateTimeOffset Time { get; init; }

        #region Derived

        public bool IsInitialized => Time != default;

        #endregion

        public readonly required TCurrency Balance { get; init; }


        public readonly required TCurrency Equity { get; init; }



        //public TPrecision Margin { get; set; } = default!;

        //public TPrecision FreeMargin { get; set; } = default!;

        //public TPrecision MarginLevel { get; set; } = default!;

        //public TPrecision Leverage { get; set; } = default!;    

        //public TPrecision MarginCallLevel { get; set; } = default!;

        //public TPrecision StopOutLevel { get; set; } = default!;

    }

#endif