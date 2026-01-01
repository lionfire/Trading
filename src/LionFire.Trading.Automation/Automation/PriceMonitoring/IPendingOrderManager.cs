using System.Numerics;
using LionFire.Trading.PriceMonitoring;

namespace LionFire.Trading.Automation.PriceMonitoring;

/// <summary>
/// Manages pending stop-loss and take-profit orders for live/simulated trading.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
/// <remarks>
/// <para>
/// The pending order manager works in conjunction with <see cref="ILivePriceMonitor"/>
/// to track orders and trigger them when price conditions are met.
/// </para>
/// <para>
/// When the first order is registered for a symbol, the manager subscribes to
/// price updates. When the last order for a symbol is removed, it unsubscribes.
/// </para>
/// </remarks>
public interface IPendingOrderManager<TPrecision> : IAsyncDisposable
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Registers a stop-loss order for a position.
    /// </summary>
    /// <param name="position">The position to attach the stop-loss to.</param>
    /// <param name="triggerPrice">The price at which to trigger the stop-loss.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes with the created pending order.</returns>
    /// <remarks>
    /// For long positions, the stop-loss triggers when price falls to or below the trigger price.
    /// For short positions, it triggers when price rises to or above the trigger price.
    /// </remarks>
    Task<PendingOrder<TPrecision>> RegisterStopLossAsync(
        IPosition<TPrecision> position,
        TPrecision triggerPrice,
        CancellationToken ct = default);

    /// <summary>
    /// Registers a take-profit order for a position.
    /// </summary>
    /// <param name="position">The position to attach the take-profit to.</param>
    /// <param name="triggerPrice">The price at which to trigger the take-profit.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes with the created pending order.</returns>
    /// <remarks>
    /// For long positions, the take-profit triggers when price rises to or above the trigger price.
    /// For short positions, it triggers when price falls to or below the trigger price.
    /// </remarks>
    Task<PendingOrder<TPrecision>> RegisterTakeProfitAsync(
        IPosition<TPrecision> position,
        TPrecision triggerPrice,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the trigger price of an existing pending order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="newTriggerPrice">The new trigger price.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes when the order is updated.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the order ID is not found.</exception>
    Task UpdateOrderAsync(string orderId, TPrecision newTriggerPrice, CancellationToken ct = default);

    /// <summary>
    /// Cancels a pending order.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the order was found and cancelled, false if not found.</returns>
    Task<bool> CancelOrderAsync(string orderId, CancellationToken ct = default);

    /// <summary>
    /// Cancels all pending orders associated with a position.
    /// </summary>
    /// <param name="positionId">The position ID whose orders should be cancelled.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of orders cancelled.</returns>
    Task<int> ClearOrdersForPositionAsync(int positionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all pending orders for a specific symbol.
    /// </summary>
    /// <param name="symbol">The exchange symbol to query.</param>
    /// <returns>An enumerable of pending orders for the symbol.</returns>
    IEnumerable<PendingOrder<TPrecision>> GetOrdersForSymbol(ExchangeSymbol symbol);

    /// <summary>
    /// Gets a pending order by its ID.
    /// </summary>
    /// <param name="orderId">The order ID to look up.</param>
    /// <returns>The pending order, or null if not found.</returns>
    PendingOrder<TPrecision>? GetOrder(string orderId);

    /// <summary>
    /// Gets all pending orders for a position.
    /// </summary>
    /// <param name="positionId">The position ID to query.</param>
    /// <returns>An enumerable of pending orders for the position.</returns>
    IEnumerable<PendingOrder<TPrecision>> GetOrdersForPosition(int positionId);

    /// <summary>
    /// Gets the total count of pending orders.
    /// </summary>
    int OrderCount { get; }

    /// <summary>
    /// Gets the count of symbols with active pending orders.
    /// </summary>
    int ActiveSymbolCount { get; }

    /// <summary>
    /// Event raised when a pending order is triggered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The event provides the triggered order and the execution price.
    /// This event is raised from the price monitoring thread.
    /// </para>
    /// <para>
    /// The order is automatically removed from the manager before this event fires.
    /// </para>
    /// </remarks>
    event Action<PendingOrder<TPrecision>, TPrecision>? OnOrderTriggered;

    /// <summary>
    /// Event raised when a pending order is registered.
    /// </summary>
    event Action<PendingOrder<TPrecision>>? OnOrderRegistered;

    /// <summary>
    /// Event raised when a pending order is cancelled.
    /// </summary>
    event Action<PendingOrder<TPrecision>>? OnOrderCancelled;

    /// <summary>
    /// Event raised when a pending order's trigger price is updated.
    /// </summary>
    event Action<PendingOrder<TPrecision>, TPrecision>? OnOrderUpdated;
}
