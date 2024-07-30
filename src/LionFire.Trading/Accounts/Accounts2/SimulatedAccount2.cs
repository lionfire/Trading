using DynamicData;
using LionFire.Trading.HistoricalData.Retrieval;
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

public abstract class SimulatedAccount2<TPrecision> : IAccount2
    where TPrecision : INumber<TPrecision>
{

    #region Identity

    public string Exchange { get; }

    public string ExchangeArea { get; }
    //public IBars Bars { get; }

    public bool IsSimulation => true;

    public bool IsRealMoney => false;

    public virtual bool IsHedging => false;

    #endregion

    #region Lifecycle

    protected SimulatedAccount2(string exchange, string exchangeArea)
    {
        Exchange = exchange;
        ExchangeArea = exchangeArea;
        //Bars = bars;
    }

    #endregion

    public MarketFeatures GetMarketFeatures(string symbol)
    {
        throw new NotImplementedException();
    }

    #region State

    public DateTimeOffset DateTime { get; protected set; }

    public IObservableCache<IPosition, int> Positions => positions;

    IEnumerable<IPosition> IAccount2.Positions => throw new NotImplementedException();

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

    #endregion

    #region Methods

    public abstract ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, double positionSize);
    public abstract ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
    public abstract ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, decimal positionSize);

    #region Close

    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, double positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

    public ValueTask<IOrderResult> ClosePosition(IPosition position) // TODO: Execution options: PostOnly, etc.
    {
        positions.Remove(position);
        return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = position }); // TODO: ClosePositionResult, with PnL
    }


    #endregion

    #endregion
}

public class AccountStateJournal<TPrecision>
    where TPrecision : INumber<TPrecision>
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

