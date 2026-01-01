using System.Numerics;

namespace LionFire.Trading.Automation.FillSimulation;

/// <summary>
/// Simulates order fills for live simulated trading.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
/// <remarks>
/// <para>
/// Fill simulators calculate the execution price for orders in paper/simulated trading.
/// Different implementations provide varying levels of realism, from simple
/// market-price fills to sophisticated slippage modeling.
/// </para>
/// <para>
/// For production use, choose a simulator based on:
/// <list type="bullet">
///   <item><description><see cref="SimpleFillSimulator{TPrecision}"/>: Fast, deterministic fills for testing</description></item>
///   <item><description><see cref="RealisticFillSimulator{TPrecision}"/>: Slippage modeling for realistic simulation</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IFillSimulator<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Calculates the fill price for an order.
    /// </summary>
    /// <param name="request">The fill calculation request containing order details.</param>
    /// <returns>The calculated fill result including execution price and fill details.</returns>
    FillResult<TPrecision> CalculateFill(FillRequest<TPrecision> request);
}

/// <summary>
/// Request containing all information needed to calculate a fill.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
public readonly record struct FillRequest<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Gets the type of order being filled.
    /// </summary>
    public required FillOrderType OrderType { get; init; }

    /// <summary>
    /// Gets the direction of the trade.
    /// </summary>
    public required LongAndShort Direction { get; init; }

    /// <summary>
    /// Gets the quantity to fill.
    /// </summary>
    public required TPrecision Quantity { get; init; }

    /// <summary>
    /// Gets the current best bid price.
    /// </summary>
    public required TPrecision Bid { get; init; }

    /// <summary>
    /// Gets the current best ask price.
    /// </summary>
    public required TPrecision Ask { get; init; }

    /// <summary>
    /// Gets the limit price (for limit orders).
    /// </summary>
    public TPrecision? LimitPrice { get; init; }

    /// <summary>
    /// Gets the stop trigger price (for stop orders).
    /// </summary>
    public TPrecision? StopPrice { get; init; }

    /// <summary>
    /// Gets the exchange symbol for additional context.
    /// </summary>
    public ExchangeSymbol? Symbol { get; init; }
}

/// <summary>
/// Result of a fill calculation.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
public readonly record struct FillResult<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Gets whether the fill was successful.
    /// </summary>
    public required bool IsFilled { get; init; }

    /// <summary>
    /// Gets the execution price.
    /// </summary>
    public required TPrecision ExecutionPrice { get; init; }

    /// <summary>
    /// Gets the filled quantity.
    /// </summary>
    public required TPrecision FilledQuantity { get; init; }

    /// <summary>
    /// Gets the unfilled quantity (for partial fills).
    /// </summary>
    public TPrecision UnfilledQuantity { get; init; }

    /// <summary>
    /// Gets the slippage applied (difference from theoretical price).
    /// </summary>
    public TPrecision Slippage { get; init; }

    /// <summary>
    /// Gets the reason if not filled or partially filled.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Creates a successful full fill result.
    /// </summary>
    public static FillResult<TPrecision> FullFill(TPrecision price, TPrecision quantity, TPrecision slippage = default)
        => new()
        {
            IsFilled = true,
            ExecutionPrice = price,
            FilledQuantity = quantity,
            UnfilledQuantity = default,
            Slippage = slippage
        };

    /// <summary>
    /// Creates a failed fill result.
    /// </summary>
    public static FillResult<TPrecision> NoFill(TPrecision quantity, string reason)
        => new()
        {
            IsFilled = false,
            ExecutionPrice = default,
            FilledQuantity = default,
            UnfilledQuantity = quantity,
            Reason = reason
        };
}

/// <summary>
/// Type of order for fill simulation.
/// </summary>
public enum FillOrderType
{
    /// <summary>
    /// Market order - fill at current market price.
    /// </summary>
    Market = 0,

    /// <summary>
    /// Limit order - fill at limit price or better.
    /// </summary>
    Limit = 1,

    /// <summary>
    /// Stop order - triggers at stop price, then fills at market.
    /// </summary>
    Stop = 2,

    /// <summary>
    /// Stop-limit order - triggers at stop price, then fills at limit.
    /// </summary>
    StopLimit = 3
}
