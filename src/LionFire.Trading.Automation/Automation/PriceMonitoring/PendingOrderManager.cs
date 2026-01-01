using System.Collections.Concurrent;
using System.Numerics;
using LionFire.Trading.PriceMonitoring;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.PriceMonitoring;

/// <summary>
/// Manages pending stop-loss and take-profit orders for live/simulated trading.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
/// <remarks>
/// <para>
/// This implementation:
/// </para>
/// <list type="bullet">
///   <item><description>Tracks orders by symbol and by ID for efficient lookup</description></item>
///   <item><description>Manages price subscriptions automatically (subscribe on first order, unsubscribe on last removal)</description></item>
///   <item><description>Processes price ticks to check for order triggers</description></item>
///   <item><description>Thread-safe for concurrent order operations</description></item>
/// </list>
/// </remarks>
public sealed class PendingOrderManager<TPrecision> : IPendingOrderManager<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Dependencies

    private readonly ILivePriceMonitor _priceMonitor;
    private readonly ILogger<PendingOrderManager<TPrecision>>? _logger;

    #endregion

    #region State

    /// <summary>
    /// Orders indexed by order ID for O(1) lookup.
    /// </summary>
    private readonly ConcurrentDictionary<string, PendingOrder<TPrecision>> _ordersById = new();

    /// <summary>
    /// Orders indexed by symbol for efficient price tick processing.
    /// </summary>
    private readonly ConcurrentDictionary<ExchangeSymbol, ConcurrentBag<PendingOrder<TPrecision>>> _ordersBySymbol = new();

    /// <summary>
    /// Orders indexed by position ID for efficient position-based queries.
    /// </summary>
    private readonly ConcurrentDictionary<int, ConcurrentBag<PendingOrder<TPrecision>>> _ordersByPosition = new();

    /// <summary>
    /// Subscription reference counts per symbol.
    /// </summary>
    private readonly ConcurrentDictionary<ExchangeSymbol, int> _subscriptionCounts = new();

    /// <summary>
    /// Lock for coordinating subscription lifecycle.
    /// </summary>
    private readonly SemaphoreSlim _subscriptionLock = new(1, 1);

    /// <summary>
    /// Counter for generating unique order IDs.
    /// </summary>
    private long _nextOrderId = 1;

    private bool _disposed;

    #endregion

    #region Events

    /// <inheritdoc />
    public event Action<PendingOrder<TPrecision>, TPrecision>? OnOrderTriggered;

    /// <inheritdoc />
    public event Action<PendingOrder<TPrecision>>? OnOrderRegistered;

    /// <inheritdoc />
    public event Action<PendingOrder<TPrecision>>? OnOrderCancelled;

    /// <inheritdoc />
    public event Action<PendingOrder<TPrecision>, TPrecision>? OnOrderUpdated;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int OrderCount => _ordersById.Count;

    /// <inheritdoc />
    public int ActiveSymbolCount => _ordersBySymbol.Count(kvp => !kvp.Value.IsEmpty);

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new pending order manager.
    /// </summary>
    /// <param name="priceMonitor">The price monitor for receiving price updates.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public PendingOrderManager(ILivePriceMonitor priceMonitor, ILogger<PendingOrderManager<TPrecision>>? logger = null)
    {
        _priceMonitor = priceMonitor ?? throw new ArgumentNullException(nameof(priceMonitor));
        _logger = logger;

        // Subscribe to price ticks
        _priceMonitor.OnPriceTick += HandlePriceTick;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // Unsubscribe from price ticks
        _priceMonitor.OnPriceTick -= HandlePriceTick;

        // Unsubscribe from all symbols
        foreach (var symbol in _subscriptionCounts.Keys)
        {
            try
            {
                await _priceMonitor.UnsubscribeAsync(symbol).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error unsubscribing from {Symbol} during disposal", symbol);
            }
        }

        _ordersById.Clear();
        _ordersBySymbol.Clear();
        _ordersByPosition.Clear();
        _subscriptionCounts.Clear();

        _subscriptionLock.Dispose();
    }

    #endregion

    #region Order Registration

    /// <inheritdoc />
    public async Task<PendingOrder<TPrecision>> RegisterStopLossAsync(
        IPosition<TPrecision> position,
        TPrecision triggerPrice,
        CancellationToken ct = default)
    {
        return await RegisterOrderAsync(position, triggerPrice, SimulatedOrderType.StopLoss, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PendingOrder<TPrecision>> RegisterTakeProfitAsync(
        IPosition<TPrecision> position,
        TPrecision triggerPrice,
        CancellationToken ct = default)
    {
        return await RegisterOrderAsync(position, triggerPrice, SimulatedOrderType.TakeProfit, ct).ConfigureAwait(false);
    }

    private async Task<PendingOrder<TPrecision>> RegisterOrderAsync(
        IPosition<TPrecision> position,
        TPrecision triggerPrice,
        SimulatedOrderType orderType,
        CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var symbol = new ExchangeSymbol(position.SymbolId.Exchange, position.SymbolId.ExchangeArea, position.Symbol);
        var orderId = $"PO-{Interlocked.Increment(ref _nextOrderId)}";

        var order = new PendingOrder<TPrecision>
        {
            Id = orderId,
            PositionId = position.Id,
            Symbol = symbol,
            OrderType = orderType,
            TriggerPrice = triggerPrice,
            Direction = position.LongOrShort
        };

        // Add to indices
        _ordersById[orderId] = order;

        var symbolOrders = _ordersBySymbol.GetOrAdd(symbol, _ => new ConcurrentBag<PendingOrder<TPrecision>>());
        symbolOrders.Add(order);

        var positionOrders = _ordersByPosition.GetOrAdd(position.Id, _ => new ConcurrentBag<PendingOrder<TPrecision>>());
        positionOrders.Add(order);

        // Manage subscription
        await EnsureSubscribedAsync(symbol, ct).ConfigureAwait(false);

        _logger?.LogDebug(
            "Registered {OrderType} order {OrderId} for position {PositionId} at {TriggerPrice}",
            orderType, orderId, position.Id, triggerPrice);

        OnOrderRegistered?.Invoke(order);

        return order;
    }

    #endregion

    #region Order Management

    /// <inheritdoc />
    public Task UpdateOrderAsync(string orderId, TPrecision newTriggerPrice, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_ordersById.TryGetValue(orderId, out var order))
        {
            throw new KeyNotFoundException($"Order {orderId} not found");
        }

        var oldPrice = order.TriggerPrice;
        order.TriggerPrice = newTriggerPrice;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        _logger?.LogDebug(
            "Updated order {OrderId} trigger price from {OldPrice} to {NewPrice}",
            orderId, oldPrice, newTriggerPrice);

        OnOrderUpdated?.Invoke(order, oldPrice);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> CancelOrderAsync(string orderId, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_ordersById.TryRemove(orderId, out var order))
        {
            return false;
        }

        // Note: ConcurrentBag doesn't support removal, so we mark as removed
        // and filter during iteration. In a production system, we'd use a different structure.
        // For now, we just leave it in the bag and skip it during processing.
        // The order is removed from _ordersById which is the authoritative source.

        await CheckUnsubscribeAsync(order.Symbol, ct).ConfigureAwait(false);

        _logger?.LogDebug("Cancelled order {OrderId}", orderId);

        OnOrderCancelled?.Invoke(order);

        return true;
    }

    /// <inheritdoc />
    public async Task<int> ClearOrdersForPositionAsync(int positionId, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_ordersByPosition.TryGetValue(positionId, out var orders))
        {
            return 0;
        }

        var cancelledCount = 0;
        var symbolsToCheck = new HashSet<ExchangeSymbol>();

        foreach (var order in orders)
        {
            if (_ordersById.TryRemove(order.Id, out var removedOrder))
            {
                cancelledCount++;
                symbolsToCheck.Add(removedOrder.Symbol);
                OnOrderCancelled?.Invoke(removedOrder);
            }
        }

        // Check if we need to unsubscribe from any symbols
        foreach (var symbol in symbolsToCheck)
        {
            await CheckUnsubscribeAsync(symbol, ct).ConfigureAwait(false);
        }

        _logger?.LogDebug("Cleared {Count} orders for position {PositionId}", cancelledCount, positionId);

        return cancelledCount;
    }

    #endregion

    #region Order Queries

    /// <inheritdoc />
    public IEnumerable<PendingOrder<TPrecision>> GetOrdersForSymbol(ExchangeSymbol symbol)
    {
        if (!_ordersBySymbol.TryGetValue(symbol, out var orders))
        {
            return Enumerable.Empty<PendingOrder<TPrecision>>();
        }

        // Filter to only return orders that still exist in the primary index
        return orders.Where(o => _ordersById.ContainsKey(o.Id));
    }

    /// <inheritdoc />
    public PendingOrder<TPrecision>? GetOrder(string orderId)
    {
        _ordersById.TryGetValue(orderId, out var order);
        return order;
    }

    /// <inheritdoc />
    public IEnumerable<PendingOrder<TPrecision>> GetOrdersForPosition(int positionId)
    {
        if (!_ordersByPosition.TryGetValue(positionId, out var orders))
        {
            return Enumerable.Empty<PendingOrder<TPrecision>>();
        }

        // Filter to only return orders that still exist in the primary index
        return orders.Where(o => _ordersById.ContainsKey(o.Id));
    }

    #endregion

    #region Price Handling

    private void HandlePriceTick(ExchangeSymbol symbol, decimal bid, decimal ask)
    {
        if (_disposed) return;

        var bidPrecision = TPrecision.CreateChecked(bid);
        var askPrecision = TPrecision.CreateChecked(ask);

        if (!_ordersBySymbol.TryGetValue(symbol, out var orders))
        {
            return;
        }

        // Check each order for trigger condition
        foreach (var order in orders)
        {
            // Skip if already removed from primary index
            if (!_ordersById.ContainsKey(order.Id))
            {
                continue;
            }

            if (order.ShouldTrigger(bidPrecision, askPrecision))
            {
                // Remove from primary index first
                if (_ordersById.TryRemove(order.Id, out _))
                {
                    var executionPrice = order.GetExecutionPrice(bidPrecision, askPrecision);

                    _logger?.LogInformation(
                        "{OrderType} order {OrderId} triggered at {ExecutionPrice} (trigger: {TriggerPrice})",
                        order.OrderType, order.Id, executionPrice, order.TriggerPrice);

                    try
                    {
                        OnOrderTriggered?.Invoke(order, executionPrice);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error in OnOrderTriggered handler for order {OrderId}", order.Id);
                    }
                }
            }
        }
    }

    #endregion

    #region Subscription Management

    private async Task EnsureSubscribedAsync(ExchangeSymbol symbol, CancellationToken ct)
    {
        await _subscriptionLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var count = _subscriptionCounts.AddOrUpdate(symbol, 1, (_, c) => c + 1);

            if (count == 1)
            {
                _logger?.LogDebug("Subscribing to price feed for {Symbol}", symbol);
                await _priceMonitor.SubscribeAsync(symbol, ct).ConfigureAwait(false);
            }
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    private async Task CheckUnsubscribeAsync(ExchangeSymbol symbol, CancellationToken ct)
    {
        await _subscriptionLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!_subscriptionCounts.TryGetValue(symbol, out var count))
            {
                return;
            }

            // Count remaining valid orders for this symbol
            var remainingOrders = GetOrdersForSymbol(symbol).Count();

            if (remainingOrders == 0)
            {
                _subscriptionCounts.TryRemove(symbol, out _);
                _logger?.LogDebug("Unsubscribing from price feed for {Symbol}", symbol);
                await _priceMonitor.UnsubscribeAsync(symbol, ct).ConfigureAwait(false);
            }
            else
            {
                _subscriptionCounts[symbol] = remainingOrders;
            }
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    #endregion
}
