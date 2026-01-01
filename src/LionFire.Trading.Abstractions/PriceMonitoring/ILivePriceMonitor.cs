using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.PriceMonitoring;

/// <summary>
/// Interface for monitoring live price feeds from exchanges.
/// </summary>
/// <remarks>
/// <para>
/// Implementations connect to exchange WebSocket feeds to receive real-time
/// price updates. Subscriptions are reference-counted, so multiple consumers
/// can subscribe to the same symbol without duplicate connections.
/// </para>
/// <para>
/// Price ticks are broadcast via the <see cref="OnPriceTick"/> event.
/// Consumers should subscribe to this event before calling <see cref="SubscribeAsync"/>.
/// </para>
/// </remarks>
public interface ILivePriceMonitor : IAsyncDisposable
{
    /// <summary>
    /// Subscribes to price updates for a specific symbol.
    /// </summary>
    /// <param name="symbol">The exchange symbol to subscribe to.</param>
    /// <param name="ct">Cancellation token for the subscription request.</param>
    /// <returns>A task that completes when the subscription is established.</returns>
    /// <remarks>
    /// If already subscribed, this increments an internal reference count.
    /// Call <see cref="UnsubscribeAsync"/> the same number of times to fully unsubscribe.
    /// </remarks>
    Task SubscribeAsync(ExchangeSymbol symbol, CancellationToken ct = default);

    /// <summary>
    /// Unsubscribes from price updates for a specific symbol.
    /// </summary>
    /// <param name="symbol">The exchange symbol to unsubscribe from.</param>
    /// <param name="ct">Cancellation token for the unsubscription request.</param>
    /// <returns>A task that completes when the unsubscription is processed.</returns>
    /// <remarks>
    /// Decrements the internal reference count. The actual WebSocket unsubscription
    /// only occurs when the count reaches zero.
    /// </remarks>
    Task UnsubscribeAsync(ExchangeSymbol symbol, CancellationToken ct = default);

    /// <summary>
    /// Gets the current bid and ask prices for a symbol.
    /// </summary>
    /// <param name="symbol">The exchange symbol to query.</param>
    /// <returns>
    /// A tuple of (Bid, Ask) prices if available, or null if not subscribed
    /// or no price data has been received yet.
    /// </returns>
    (decimal? Bid, decimal? Ask)? GetCurrentPrice(ExchangeSymbol symbol);

    /// <summary>
    /// Checks whether the monitor is subscribed to a specific symbol.
    /// </summary>
    /// <param name="symbol">The exchange symbol to check.</param>
    /// <returns>True if subscribed to the symbol, false otherwise.</returns>
    bool IsSubscribed(ExchangeSymbol symbol);

    /// <summary>
    /// Gets the subscription reference count for a symbol.
    /// </summary>
    /// <param name="symbol">The exchange symbol to check.</param>
    /// <returns>The number of active subscriptions for the symbol.</returns>
    int GetSubscriptionCount(ExchangeSymbol symbol);

    /// <summary>
    /// Event raised when a new price tick is received.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The event provides the symbol, bid price, and ask price.
    /// This event may be raised from a background thread.
    /// </para>
    /// <para>
    /// Handlers should be fast and non-blocking to avoid
    /// slowing down price feed processing.
    /// </para>
    /// </remarks>
    event Action<ExchangeSymbol, decimal, decimal>? OnPriceTick;
}
