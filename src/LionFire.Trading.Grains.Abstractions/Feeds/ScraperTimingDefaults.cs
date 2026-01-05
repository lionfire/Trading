namespace LionFire.Trading.Feeds;

/// <summary>
/// Default timing values for the demand-driven bar scraping architecture.
/// These constants define the lease/heartbeat pattern that ensures scrapers
/// shut down when subscribers are gone, even after silo crashes.
/// </summary>
/// <remarks>
/// <para>
/// The timing relationship must satisfy: HeartbeatInterval &lt; LeaseTimeout &gt; ObserverSubscriptionDuration
/// </para>
/// <para>
/// This ensures:
/// - Multiple heartbeats per lease period for resilience
/// - Scrapers outlive subscriber observer timeouts for explicit deactivation
/// - Orphaned scrapers eventually shut down
/// </para>
/// </remarks>
public static class ScraperTimingDefaults
{
    /// <summary>
    /// Duration after which a subscriber's lease expires if not renewed via Heartbeat.
    /// Set slightly higher than <see cref="ObserverSubscriptionDuration"/> to allow
    /// subscribers time to detect stale observers and explicitly deactivate scrapers.
    /// </summary>
    public static readonly TimeSpan LeaseTimeout = TimeSpan.FromMinutes(6);

    /// <summary>
    /// Interval for checking expired subscriber leases in the scraper.
    /// Should be frequent enough to detect expired leases promptly.
    /// </summary>
    public static readonly TimeSpan LeaseCheckInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Interval for sending heartbeats from subscriber (e.g., LastBarsG) to scraper.
    /// Should be well under <see cref="LeaseTimeout"/> to allow for retries on transient failures.
    /// Recommended: At least 3 heartbeats per lease period.
    /// </summary>
    public static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Default observer subscription duration for bar subscribers (e.g., LastBarsG).
    /// This is how long a client's subscription to LastBarsG remains valid without renewal.
    /// Must be less than <see cref="LeaseTimeout"/> for proper cleanup ordering.
    /// </summary>
    public static readonly TimeSpan ObserverSubscriptionDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Interval for the MarketPrewarmServiceG reminder to call EnsureActive on priority markets.
    /// </summary>
    public static readonly TimeSpan PrewarmInterval = TimeSpan.FromMinutes(5);
}
