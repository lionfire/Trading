using DynamicData;
using System.Numerics;

namespace LionFire.Trading.Automation;

public class PBacktestAccount<T>
    where T : INumber<T>
{

    #region (static)

    public static PBacktestAccount<T> Default { get; }

    static PBacktestAccount()
    {
        if (typeof(T) == typeof(double))
        {
            Default = (PBacktestAccount<T>)(object)new PBacktestAccount<double>()
            {
                StartingBalance = 10_000.0
            };
        }
        else if (typeof(T) == typeof(decimal))
        {
            Default = (PBacktestAccount<T>)(object)new PBacktestAccount<decimal>()
            {
                StartingBalance = 10_000m
            };
        }
        else
        {
            Default = (PBacktestAccount<T>)Activator.CreateInstance(typeof(PBacktestAccount<T>), [default(T)])!;
        }
    }

    #endregion

    #region Lifecycle

    public PBacktestAccount() { }
    public PBacktestAccount(T startingBalance)
    {
        StartingBalance = startingBalance;
    }

    #endregion

    public required T StartingBalance { get; set; }
}

public class BacktestAccount2<TPrecision> : SimulatedAccount2<TPrecision>
    where TPrecision : INumber<TPrecision>
{

    public PBacktestAccount<TPrecision> Parameters => parameters ?? PBacktestAccount<TPrecision>.Default;
    private PBacktestAccount<TPrecision>? parameters;

    #region Accounts

    public BacktestBotController BacktestBotController { get; }

    #endregion

    #region Lifecycle

    public BacktestAccount2(BacktestBotController backtestBotController, string exchange, string exchangeArea, PBacktestAccount<TPrecision>? parameters = null) : base(exchange, exchangeArea)
    {
        BacktestBotController = backtestBotController;
        this.parameters = parameters;
    }

    #endregion

    #region Methods

    int positionIdCounter = 0;


    public override ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, double positionSize)
    {
        var p = new PositionBase()
        {
            Id = positionIdCounter++,
            //EntryTime = ,            
            EntryPrice = 0,
            Quantity = (decimal)positionSize,
            SymbolId = new SymbolId { Symbol = symbol },
            TakeProfit = null,
            StopLoss = null,
        };
        positions.AddOrUpdate(p);
        return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = p });
    }

    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, double positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }
    public override ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) { throw new NotImplementedException(); }
    public override ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, decimal positionSize) { throw new NotImplementedException(); }
    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }

    #endregion

}
