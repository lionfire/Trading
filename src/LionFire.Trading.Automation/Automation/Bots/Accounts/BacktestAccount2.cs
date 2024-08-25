//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
using DynamicData;
using LionFire.Structures;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Backtesting;
using LionFire.Trading.ValueWindows;
using System.Diagnostics;
using System.Numerics;

namespace LionFire.Trading.Automation;

public readonly record struct InstanceInputInfo(IPInput PInput, InputInjectionInfo TypeInputInfo);

public interface IHasInstanceInputInfos
{
    public List<InstanceInputInfo> InstanceInputInfos { get; }
    public object Instance { get; }
}


public class PBacktestAccount<TPrecision>
#if BacktestAccountSlottedParameters
    : SlottedParameters<BacktestAccount2<T>>
    , IPTimeFrameMarketProcessor
#else
    : IPMayHaveUnboundInputSlots
#endif
    , IPTimeFrameMarketProcessor
    , IParametersFor<BacktestAccount2<TPrecision>>
    , IPAccount2
    , ICloneable
    where TPrecision : struct, INumber<TPrecision>
{

    #region (static)

    public static PBacktestAccount<TPrecision> Default { get; }

    static PBacktestAccount()
    {
        if (typeof(TPrecision) == typeof(double))
        {
            Default = (PBacktestAccount<TPrecision>)(object)new PBacktestAccount<double>(10_000.0)
            {
                //StartingBalance = 
            };
        }
        else if (typeof(TPrecision) == typeof(decimal))
        {
            Default = (PBacktestAccount<TPrecision>)(object)new PBacktestAccount<decimal>(10_000m)
            {
                //StartingBalance = 
            };
        }
        else
        {
            Default = (PBacktestAccount<TPrecision>)Activator.CreateInstance(typeof(PBacktestAccount<TPrecision>), [default(TPrecision)])!;
        }
    }

    #endregion

    #region Lifecycle

    //public PBacktestAccount() { }
    public PBacktestAccount(TPrecision startingBalance)
    {
        StartingBalance = startingBalance;
    }

    #endregion

#if BacktestAccountSlottedParameters
    // Get slots using: InputSlotsReflection.GetInputSlots(this.GetType());
    //[Slot(0)]
#endif
    /// <summary>
    /// If null, populate from base.DefaultSymbol, if set
    /// </summary>
    public HLCReference<TPrecision>? Bars { get; set; }

    public TPrecision StartingBalance { get; set; }

    public TimeFrame TimeFrame { get; set; }

    public int[]? InputLookbacks => [1];
    public Type MaterializedType => typeof(BacktestAccount2<double>);

    public IReadOnlyList<InputSlot> InputSlots => InputSlotsReflection.GetInputSlots(typeof(PBacktestAccount<TPrecision>));

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public class BacktestAccount2<TPrecision>
    : SimulatedAccount2<TPrecision>
    , IHasSignalInfo
    , IHasInstanceInputInfos
    where TPrecision : struct, INumber<TPrecision>
{

    public new PBacktestAccount<TPrecision> Parameters => base.Parameters as PBacktestAccount<TPrecision> ?? PBacktestAccount<TPrecision>.Default;
    //private PBacktestAccount<TPrecision>? parameters;

    #region Inputs

    public IReadOnlyValuesWindow<HLC<TPrecision>> Bars { get; set; } = null!;

    IReadOnlyList<SignalInfo> IHasSignalInfo.GetSignalInfos() => signalInfos ??= [new SignalInfo(typeof(BacktestAccount2<TPrecision>).GetProperty(nameof(Bars))!)];
    IReadOnlyList<SignalInfo> signalInfos;

    #endregion

    #region Relationships

    public BacktestBotController<double> BacktestBotController { get; }

    static BotInfo BotInfo => BotInfos.Get(typeof(PBacktestAccount<TPrecision>), typeof(BacktestAccount2<TPrecision>));

    List<InstanceInputInfo> IHasInstanceInputInfos.InstanceInputInfos
//=>        [];
=> [new(Parameters.Bars!, BotInfo.InputInjectionInfos![0])];

    object IHasInstanceInputInfos.Instance => this;

    #endregion

    #region Lifecycle

    public BacktestAccount2(PBacktestAccount<TPrecision> parameters, BacktestBotController<double> backtestBotController, string exchange, string exchangeArea, string? symbol = null) : base(parameters, exchange, exchangeArea, symbol)
    {
        BacktestBotController = backtestBotController;

        DateTime = backtestBotController.BotBatchController.Start;
    }

    #endregion

    #region Methods

    int positionIdCounter = 0;

    TPrecision CurrentPrice(string symbol)
    {
        if (symbol == Parameters.Bars.Symbol)
        {
            return Bars[0].Close;
        }
        else
        {
            throw new NotImplementedException("symbol other than the default symbol: " + Parameters.Bars.Symbol);
        }
    }

    public override ValueTask<IOrderResult> ExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision positionSize)
    {
        var p = new PositionBase<TPrecision>(this, symbol)
        {
            Id = positionIdCounter++,
            //EntryTime = ,            
            EntryPrice = CurrentPrice(symbol),
            Quantity = positionSize,
            TakeProfit = default,
            StopLoss = default,
        };
        positions.AddOrUpdate(p);
        return ValueTask.FromResult<IOrderResult>(new OrderResult { IsSuccess = true, Data = p });
    }

    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }
    public override ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) { throw new NotImplementedException(); }
    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }

    #endregion

    #region Event Handlers

    int x = 0;
    public override void OnBar()
    {
        if (x++ <= 0)
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
        if (x % 10000 == 0)
        {
            Debug.WriteLine($"#{x} Account - {Bars.Size} bars, {Bars[0]}");
        }

    }
    #endregion

}
