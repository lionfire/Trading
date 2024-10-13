using DynamicData;
using LionFire.Threading;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.ValueWindows;
using System.CommandLine;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Automation;

public interface IPriceMonitorStateMachine
{
}

public class PriceMonitorStateMachine : IPriceMonitorStateMachine
{
    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }

    public PriceMonitorStateMachine(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
    }
}

public enum AbortReason
{
    Unspecified,
    BalanceDrawdown,
    EquityDrawdown,
}

public abstract class SimulatedAccount2<TPrecision> : ISimulatedAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{

    IPAccount2 IAccount2.Parameters => Parameters;
    public IPSimulatedAccount2<TPrecision> Parameters { get; }

    #region Identity

    public string Exchange { get; }

    public string ExchangeArea { get; }
    public string? DefaultSymbol { get; }

    //public IBars Bars { get; }

    public bool IsSimulation => true;

    public bool IsRealMoney => false;

    public virtual bool IsHedging => false;

    #endregion

    #region Relationships

    public ISimulationController<TPrecision> Controller { get; protected set; }

    #endregion

    #region Lifecycle

    protected SimulatedAccount2(IPSimulatedAccount2<TPrecision> parameters, ISimulationController<TPrecision> controller, string exchange, string exchangeArea, string? defaultSymbol = null)
    {
        Controller = controller;
        Parameters = parameters;
        Exchange = exchange;
        ExchangeArea = exchangeArea;
        DefaultSymbol = defaultSymbol;
        Balance = parameters.StartingBalance;
        //Bars = bars;
    }

    #endregion

    public MarketFeatures GetMarketFeatures(string symbol)
    {
        throw new NotImplementedException();
    }

    #region State

    public TPrecision Balance
    {
        get => balance;

        protected set
        {
            balance = value;
            if (balance > HighestBalance) { HighestBalance = balance; }
            else
            {
                var drawdown = CurrentBalanceDrawdown;
                if (drawdown > MaxBalanceDrawdown) { MaxBalanceDrawdown = drawdown; }
                var perunum = drawdown / HighestBalance;
                if (perunum > MaxBalanceDrawdownPerunum) { MaxBalanceDrawdownPerunum = perunum; }
                if (Parameters.AbortOnBalanceDrawdownPerunum != default && perunum > Parameters.AbortOnBalanceDrawdownPerunum) { Abort(AbortReason.BalanceDrawdown); }
            }
        }
    }
    private TPrecision balance;

    public AbortReason AbortReason { get; protected set; }
    private void Abort(AbortReason abortReason)
    {
        AbortReason = abortReason;
        IsAborted = true;
        End = Controller.SimulatedCurrentDate;
        Controller.OnAccountAborted();
    }

    public TPrecision HighestBalance { get; protected set; }

    public TPrecision Equity // OPTIMIZE: defer equity calculations until a 2nd pass once ending balance and balance drawdown is acceptable
    {
        get => equity;
        protected set
        {
            equity = value;
            if (equity > HighestEquity) { HighestEquity = equity; }
            else
            {
                var drawdown = CurrentEquityDrawdown;
                if (drawdown > MaxEquityDrawdown) { MaxEquityDrawdown = drawdown; }
                var percent = drawdown / HighestEquity;
                if (percent > MaxEquityDrawdownPercent) { MaxEquityDrawdownPercent = percent; }
            }
        }
    }
    private TPrecision equity;
    public TPrecision HighestEquity { get; protected set; }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool IsAborted { get; set; }

    #region Derived

    public TPrecision CurrentBalanceDrawdown => HighestBalance - Balance;
    public TPrecision MaxBalanceDrawdown { get; set; }
    public TPrecision MaxBalanceDrawdownPerunum { get; set; }
    public TPrecision CurrentEquityDrawdown => Equity - HighestEquity;
    public TPrecision MaxEquityDrawdown { get; set; }
    public TPrecision MaxEquityDrawdownPercent { get; set; }

    public TPrecision InitialBalance => Parameters.StartingBalance;
    public TPrecision BalanceReturnOnInvestment => (Balance - InitialBalance) / InitialBalance;
    public TPrecision EquityReturnOnInvestment => (Equity - InitialBalance) / InitialBalance;
    public double AnnualizedBalanceReturnOnInvestment => Convert.ToDouble(BalanceReturnOnInvestment) * ((DateTime - Start).TotalDays / 365);
    public double AnnualizedEquityReturnOnInvestment => Convert.ToDouble(EquityReturnOnInvestment) * ((DateTime - Start).TotalDays / 365);
    public double AnnualizedReturnOnInvestmentVsDrawdownPercent => AnnualizedEquityReturnOnInvestment / Convert.ToDouble(MaxEquityDrawdownPercent);
    public double AnnualizedBalanceReturnOnInvestmentVsDrawdownPercent => AnnualizedBalanceReturnOnInvestment / Convert.ToDouble(MaxBalanceDrawdownPerunum);

    #endregion

    public DateTimeOffset DateTime => Controller.SimulatedCurrentDate;

    public IObservableCache<IPosition<TPrecision>, int> Positions => positions;

    //IReadOnlyList<IPosition<TPrecision>> IAccount2<TPrecision>.Positions => throw new NotImplementedException();

    protected SourceCache<IPosition<TPrecision>, int> positions = new(p => p.Id);

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

    public virtual void OnBar() { }

    public void OnRealizedProfit(TPrecision realizedGrossProfitDelta)
    {
        Balance += realizedGrossProfitDelta;
    }

    #endregion

    #region Methods

    public ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified) => SimulatedExecuteMarketOrder(symbol, longAndShort, positionSize, increasePositionFlags, existingPositionId, transactionId, journalFlags: journalFlags);

    public abstract ValueTask<IOrderResult> SimulatedExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, TPrecision? currentPrice = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified);
    public abstract ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);

    #region Close

    // TODO: Change decimal to TPrecision
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    // TODO: Change decimal to TPrecision
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);


    public long GetNextTransactionId() => nextTransactionId++;
    private long nextTransactionId = 1;

    protected virtual TPrecision CurrentPrice(string symbol) => throw new NotImplementedException("No way to get price for symbol: " + symbol);

    public ValueTask<IOrderResult> ClosePosition(IPosition<TPrecision> position, JournalEntryFlags flags = JournalEntryFlags.Unspecified) => ClosePosition((PositionBase<TPrecision>)position, flags, currentPrice: null);

    protected void OnClosingPosition(PositionBase<TPrecision> position, JournalEntryFlags journalFlags, TPrecision? currentPrice, TPrecision quantityChange, TPrecision realizedGrossProfitDelta, long transactionId) // TODO: Execution options: PostOnly, etc.
    {
        Controller.Journal.Write(new JournalEntry<TPrecision>(position)
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

    public async ValueTask<IOrderResult> ClosePosition(PositionBase<TPrecision> position, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified, TPrecision? currentPrice = null) // TODO: Execution options: PostOnly, etc.
    {
        currentPrice ??= CurrentPrice(position.Symbol);

        TPrecision realizedGrossProfitDelta = position.ProfitAtPrice(currentPrice.Value);
        position.RealizedGrossProfit += realizedGrossProfitDelta;
        position.Account.OnRealizedProfit(realizedGrossProfitDelta);

        var oldQuantity = position.Quantity;
        position.Quantity = TPrecision.Zero;

        OnClosingPosition(position, journalFlags, currentPrice, -oldQuantity, realizedGrossProfitDelta, Controller.GetNextTransactionId());

        //return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = position }); // TODO: ClosePositionResult, with PnL
        return new OrderResult { IsSuccess = true, Data = position }; // TODO: ClosePositionResult, with PnL
    }

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

    #endregion

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

    public class AccountStateJournal<TPrecision>
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
}

