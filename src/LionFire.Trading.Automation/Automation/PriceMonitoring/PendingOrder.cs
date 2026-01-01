using System.Numerics;
using LionFire.Trading.PriceMonitoring;

namespace LionFire.Trading.Automation.PriceMonitoring;

/// <summary>
/// Defines the type of simulated pending order for live simulated trading.
/// </summary>
/// <remarks>
/// This is distinct from <see cref="LionFire.Trading.PendingOrderType"/> which is used for
/// exchange-level limit/stop orders. This enum is for internal simulated SL/TP tracking.
/// </remarks>
public enum SimulatedOrderType
{
    /// <summary>
    /// A stop-loss order that closes a position when price moves against it.
    /// </summary>
    /// <remarks>
    /// For long positions: triggers when price falls to or below the trigger price.
    /// For short positions: triggers when price rises to or above the trigger price.
    /// </remarks>
    StopLoss = 0,

    /// <summary>
    /// A take-profit order that closes a position when price reaches a target.
    /// </summary>
    /// <remarks>
    /// For long positions: triggers when price rises to or above the trigger price.
    /// For short positions: triggers when price falls to or below the trigger price.
    /// </remarks>
    TakeProfit = 1
}

/// <summary>
/// Represents a pending stop-loss or take-profit order that triggers on price.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
/// <remarks>
/// <para>
/// Pending orders are managed by <see cref="IPendingOrderManager{TPrecision}"/>
/// and are triggered automatically when price conditions are met.
/// </para>
/// <para>
/// Unlike exchange orders, these are internal simulated orders that execute
/// at the trigger price in paper/simulated trading.
/// </para>
/// </remarks>
public sealed class PendingOrder<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Gets the unique identifier for this order.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the position ID this order is associated with.
    /// </summary>
    public required int PositionId { get; init; }

    /// <summary>
    /// Gets the exchange symbol for this order.
    /// </summary>
    public required ExchangeSymbol Symbol { get; init; }

    /// <summary>
    /// Gets the type of this pending order (StopLoss or TakeProfit).
    /// </summary>
    public required SimulatedOrderType OrderType { get; init; }

    /// <summary>
    /// Gets or sets the trigger price for this order.
    /// </summary>
    /// <remarks>
    /// The order triggers when the market price reaches this level,
    /// based on the order type and position direction.
    /// </remarks>
    public required TPrecision TriggerPrice { get; set; }

    /// <summary>
    /// Gets the direction of the associated position (Long or Short).
    /// </summary>
    /// <remarks>
    /// This affects how the trigger condition is evaluated:
    /// <list type="bullet">
    ///   <item><description>Long + StopLoss: triggers at or below trigger price</description></item>
    ///   <item><description>Long + TakeProfit: triggers at or above trigger price</description></item>
    ///   <item><description>Short + StopLoss: triggers at or above trigger price</description></item>
    ///   <item><description>Short + TakeProfit: triggers at or below trigger price</description></item>
    /// </list>
    /// </remarks>
    public required LongAndShort Direction { get; init; }

    /// <summary>
    /// Gets the timestamp when this order was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this order was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Checks if the order should trigger based on the current price.
    /// </summary>
    /// <param name="bid">The current bid price.</param>
    /// <param name="ask">The current ask price.</param>
    /// <returns>True if the order should trigger, false otherwise.</returns>
    public bool ShouldTrigger(TPrecision bid, TPrecision ask)
    {
        // Use bid for long exit (selling), ask for short exit (buying to cover)
        var relevantPrice = Direction == LongAndShort.Long ? bid : ask;

        return OrderType switch
        {
            // StopLoss: Long exits when price drops TO or BELOW stop
            //          Short exits when price rises TO or ABOVE stop
            SimulatedOrderType.StopLoss => Direction == LongAndShort.Long
                ? relevantPrice <= TriggerPrice
                : relevantPrice >= TriggerPrice,

            // TakeProfit: Long exits when price rises TO or ABOVE target
            //            Short exits when price drops TO or BELOW target
            SimulatedOrderType.TakeProfit => Direction == LongAndShort.Long
                ? relevantPrice >= TriggerPrice
                : relevantPrice <= TriggerPrice,

            _ => false
        };
    }

    /// <summary>
    /// Gets the price at which this order would execute.
    /// </summary>
    /// <param name="bid">The current bid price.</param>
    /// <param name="ask">The current ask price.</param>
    /// <returns>The execution price based on position direction.</returns>
    public TPrecision GetExecutionPrice(TPrecision bid, TPrecision ask)
    {
        // Long positions sell at bid, short positions buy at ask
        return Direction == LongAndShort.Long ? bid : ask;
    }
}
