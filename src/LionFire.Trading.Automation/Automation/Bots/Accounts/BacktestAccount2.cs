//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
using CryptoExchange.Net.CommonObjects;
using DynamicData;
using LionFire.Assets;
using LionFire.Threading;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Journal;
using LionFire.Trading.ValueWindows;
using Polly;
using System.Diagnostics;
using System.Numerics;

namespace LionFire.Trading.Automation;

public readonly record struct InstanceInputInfo(IPInput PInput, InputInjectionInfo TypeInputInfo);

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


    static BotInfo BotInfo => BotInfos.Get(typeof(PBacktestAccount<TPrecision>), typeof(BacktestAccount2<TPrecision>));

    List<InstanceInputInfo> IHasInstanceInputInfos.InstanceInputInfos
//=>        [];
=> [new(Parameters.Bars!, BotInfo.InputInjectionInfos![0])];

    object IHasInstanceInputInfos.Instance => this;

    #endregion

    #region Lifecycle

    public BacktestAccount2(PBacktestAccount<TPrecision> parameters, BacktestBotController<TPrecision> controller, string exchange, string exchangeArea, string? symbol = null) : base(parameters, controller, exchange, exchangeArea, symbol)
    {
        //DateTime = controller.BotBatchController.Start;
    }

    #endregion

    #region Methods

    int positionIdCounter = 0;

    protected override TPrecision CurrentPrice(string symbol)
    {
        if (symbol == Parameters.Bars.Symbol)
        {
            return Bars[0].Close;
        }

        return base.CurrentPrice(symbol);
    }

    public const bool CanPositionChangeDirections = false;

    // ENH Idea: break this up into atomic operations. E.g.:
    // - reduce position 1 to 0 (close)
    // - open new position in the amount of x.
    // - transaction:
    //   - transaction Id
    //   - aggregate result
    //   - ENH: all or nothing support
    public override async ValueTask<IOrderResult> SimulatedExecuteMarketOrder(string symbol, LongAndShort longAndShort, TPrecision requestedQuantityChange, PositionOperationFlags operationFlags = PositionOperationFlags.Default, int? existingPositionId = null, long? transactionId = null, TPrecision? currentPrice = null, JournalEntryFlags journalFlags = JournalEntryFlags.Unspecified)
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

        transactionId ??= Controller.GetNextTransactionId();
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
                foreach (var position in positions.KeyValues.Select(kvp => kvp.Value).OfType<PositionBase<TPrecision>>()
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

                            Controller.Journal.Write(new JournalEntry<TPrecision>(position)
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

                        Controller.Journal.Write(new JournalEntry<TPrecision>(position)
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
                var p = new SimulatedPosition<TPrecision>(this, symbol)
                {
                    Id = positionIdCounter++,
                    EntryAverage = currentPrice.Value,
                    Quantity = requestedQuantityChangeRemaining,
                    TakeProfit = default,
                    StopLoss = default,
                    Symbol = symbol,
                };
                positions.AddOrUpdate(p);

                Controller.Journal.Write(new JournalEntry<TPrecision>(p)
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

    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, TPrecision positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }
    public override ValueTask<IOrderResult> ReducePositionForSymbol(string symbol, LongAndShort longAndShort, double positionSize) { throw new NotImplementedException(); }
    public override IAsyncEnumerable<IOrderResult> ClosePositionsForSymbol(string symbol, LongAndShort longAndShort, decimal positionSize, bool postOnly = false, decimal? marketExecuteAtPrice = null, (decimal? stop, decimal? limit)? stopLimit = null) { throw new NotImplementedException(); }

    #endregion

    #region Event Handlers

    int x = 0;
    public override async void OnBar()
    {
        foreach (var position in Positions.Items.OfType<PositionBase<TPrecision>>())
        {
            if ((position.StopLoss != default &&
                       (
                        (position.LongOrShort == LongAndShort.Long && Bars[0].Low <= position.StopLoss)
                        || (position.LongOrShort == LongAndShort.Short && Bars[0].High >= position.StopLoss)
                       )
                   )
              )
            {
                await ClosePosition(position, JournalEntryFlags.StopLoss, currentPrice: position.StopLoss.Value);
            }
            if ((position.TakeProfit != default &&
                     (
                      (position.LongOrShort == LongAndShort.Long && Bars[0].High >= position.TakeProfit)
                      || (position.LongOrShort == LongAndShort.Short && Bars[0].Low <= position.TakeProfit)
                     )
                 )
              )
            {
                await ClosePosition(position, JournalEntryFlags.TakeProfit, currentPrice: position.TakeProfit.Value);
            }
        }

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
