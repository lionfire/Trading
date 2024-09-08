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

public abstract class SimulatedAccount2<TPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    IPAccount2 IAccount2.Parameters => Parameters;
    public IPAccount2<TPrecision> Parameters { get; }

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

    protected SimulatedAccount2(IPAccount2<TPrecision> parameters, ISimulationController<TPrecision> controller, string exchange, string exchangeArea, string? defaultSymbol = null)
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

    public TPrecision Balance { get; protected set; }

    public TPrecision HighestBalance { get; protected set; }
    public TPrecision Equity { get; protected set; }
    public TPrecision HighestEquity { get; protected set; }

    public DateTimeOffset DateTime => Controller.SimulatedCurrentDate;

    public IObservableCache<IPosition, int> Positions => positions;

    //IReadOnlyList<IPosition<TPrecision>> IAccount2<TPrecision>.Positions => throw new NotImplementedException();

    protected SourceCache<IPosition, int> positions = new(p => p.Id);

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

    public abstract ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize, PositionOperationFlags increasePositionFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null);

    public abstract ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);

    #region Close

    // TODO: Change decimal to TPrecision
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    // TODO: Change decimal to TPrecision
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);


    public long GetNextTransactionId() => nextTransactionId++;
    private long nextTransactionId = 1;

    protected virtual TPrecision CurrentPrice(string symbol)
    {
        throw new NotImplementedException("No way to get price for symbol: " + symbol);
    }
    public async ValueTask<IOrderResult> ClosePosition(IPosition position) // TODO: Execution options: PostOnly, etc.
    {
        positions.Remove(position);

        var casted = (IPosition<TPrecision>)position  ;

        var ExitPrice = CurrentPrice(position.Symbol);

        await Controller.Journal.Write(new JournalEntry<TPrecision>
        {
            Time = DateTime,
            EntryType = JournalEntryType.ClosePosition,
            Symbol = position.Symbol,
            PositionId = position.Id,
            TransactionId = nextTransactionId++,
            Quantity = casted.Quantity,
            Price = ExitPrice,
        })
            ;
        //.FireAndForget();

        //return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = position }); // TODO: ClosePositionResult, with PnL
        return new OrderResult { IsSuccess = true, Data = position }; // TODO: ClosePositionResult, with PnL
    }


    #endregion

    #endregion
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

