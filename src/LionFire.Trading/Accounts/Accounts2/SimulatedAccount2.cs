using DynamicData;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Automation;

public abstract class SimulatedAccount2<TPrecision> : IAccount2
    where TPrecision : INumber<TPrecision>
{

    #region Identity

    public string Exchange { get; }

    public string ExchangeArea { get; }

    public bool IsSimulation => true;

    public bool IsRealMoney => false;

    public virtual bool IsHedging => false;

    #endregion

    #region Lifecycle

    protected SimulatedAccount2(string exchange, string exchangeArea)
    {
        Exchange = exchange;
        ExchangeArea = exchangeArea;
    }

    #endregion

    public MarketFeatures GetMarketFeatures(string symbol)
    {
        throw new NotImplementedException();
    }

    #region State

    public IEnumerable<IPosition> Positions => positions.Items;
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

    #region Methods

    public abstract ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, double positionSize);
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, double positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);
    public abstract ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize);
    public abstract ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, decimal positionSize);
    public abstract IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null);

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
