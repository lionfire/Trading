namespace LionFire.Trading.Feeds;

/// <summary>
/// Base interface for bar scraper grains that support the demand-driven architecture.
/// Exchange-specific scrapers (Binance, Phemex, Hyperliquid, etc.) should implement this interface.
/// </summary>
/// <remarks>
/// <para>
/// Grain key format: "{Symbol}^{TimeFrameString}" (e.g., "BTCUSDT^m1")
/// </para>
/// <para>
/// Scrapers implement a lease-based subscription pattern:
/// - Subscribers call <see cref="Activate"/> to register and start polling
/// - Subscribers periodically call <see cref="Heartbeat"/> to renew their lease
/// - When all leases expire (or <see cref="Deactivate"/> is called), polling stops
/// </para>
/// <para>
/// See <see cref="ScraperTimingDefaults"/> for timing constants.
/// </para>
/// </remarks>
public interface IBarScraperG : IGrainWithStringKey
{
    /// <summary>
    /// Activates the scraper with a specific subscriber grain key.
    /// The scraper will call <see cref="IBarSubscriber.OnBars"/> when new bars are retrieved.
    /// </summary>
    /// <param name="subscriberGrainKey">The grain key of the subscriber (e.g., LastBarsG) to receive bar updates.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Activate(string subscriberGrainKey);

    /// <summary>
    /// Sends a heartbeat to renew the subscriber's lease.
    /// Subscribers must call this periodically (see <see cref="ScraperTimingDefaults.HeartbeatInterval"/>)
    /// to prevent automatic deactivation after <see cref="ScraperTimingDefaults.LeaseTimeout"/>.
    /// </summary>
    /// <param name="subscriberGrainKey">The grain key of the subscriber renewing its lease.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This is essential for distributed system robustness. If a subscriber's silo crashes,
    /// OnDeactivateAsync won't be called. The heartbeat/lease pattern ensures orphaned
    /// scrapers will automatically shut down when leases expire.
    /// </remarks>
    Task Heartbeat(string subscriberGrainKey);

    /// <summary>
    /// Deactivates the scraper, removing the specified subscriber and stopping polling if no subscribers remain.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Deactivate();

    /// <summary>
    /// Returns whether the scraper is currently active and polling.
    /// </summary>
    /// <returns>True if the scraper has at least one active subscriber, false otherwise.</returns>
    Task<bool> IsActive();

    /// <summary>
    /// Manually triggers bar retrieval from the exchange.
    /// </summary>
    /// <returns>A task representing the asynchronous retrieval operation.</returns>
    Task RetrieveBars();
}
