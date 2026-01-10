using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Feeds;

/// <summary>
/// Abstract base class for bar scraper grains implementing the demand-driven architecture.
/// Provides lease-based subscriber tracking, heartbeat handling, and bar delivery.
/// </summary>
/// <remarks>
/// <para>
/// Exchange-specific scrapers (Binance, Phemex, Hyperliquid, etc.) should inherit from this class
/// and implement the abstract methods for exchange-specific polling and timer management.
/// </para>
/// <para>
/// This base class handles:
/// - Subscriber lease tracking with automatic expiration
/// - Heartbeat processing for lease renewal
/// - Bar delivery to multiple subscribers
/// - Automatic shutdown when no subscribers remain
/// </para>
/// <para>
/// See <see cref="ScraperTimingDefaults"/> for timing constants used by the lease system.
/// </para>
/// </remarks>
public abstract class BarScraperGBase : Grain, IBarScraperG
{
    #region Timing Constants

    // Timing constants from shared defaults - see ScraperTimingDefaults for rationale
    private static TimeSpan LeaseTimeout => ScraperTimingDefaults.LeaseTimeout;
    private static TimeSpan LeaseCheckInterval => ScraperTimingDefaults.LeaseCheckInterval;

    #endregion

    #region Dependencies

    /// <summary>
    /// Logger for this scraper instance.
    /// </summary>
    protected abstract ILogger Logger { get; }

    #endregion

    #region Lease Tracking

    /// <summary>
    /// Registry of subscribers with their lease expiration times.
    /// Key: subscriber grain key, Value: expiration time (UTC)
    /// </summary>
    private readonly Dictionary<string, DateTime> _subscriberLeases = new();

    /// <summary>
    /// Cached subscriber grain references for direct bar delivery.
    /// </summary>
    private readonly Dictionary<string, IBarSubscriber> _subscribers = new();

    /// <summary>
    /// Timer for checking and clearing expired subscriber leases.
    /// </summary>
    private IDisposable? _leaseCheckTimer;

    /// <summary>
    /// Gets the current subscriber count.
    /// </summary>
    protected int SubscriberCount => _subscriberLeases.Count;

    /// <summary>
    /// Gets whether there are any active subscribers.
    /// </summary>
    protected bool HasSubscribers => _subscriberLeases.Count > 0;

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Called when the first subscriber activates. Start polling for bars.
    /// </summary>
    protected abstract void OnFirstSubscriber();

    /// <summary>
    /// Called when the last subscriber leaves. Stop polling for bars.
    /// </summary>
    protected abstract Task OnLastSubscriberRemoved();

    /// <summary>
    /// Gets a unique identifier for this grain (used in log messages).
    /// </summary>
    protected abstract string GrainId { get; }

    #endregion

    #region IBarScraperG Implementation

    /// <inheritdoc/>
    public virtual Task Activate(string subscriberGrainKey)
    {
        var isNewSubscriber = !_subscriberLeases.ContainsKey(subscriberGrainKey);
        var wasEmpty = _subscriberLeases.Count == 0;

        // Set or renew the lease
        _subscriberLeases[subscriberGrainKey] = DateTime.UtcNow.Add(LeaseTimeout);

        // Cache the subscriber reference if new
        if (isNewSubscriber)
        {
            _subscribers[subscriberGrainKey] = GrainFactory.GetGrain<IBarSubscriber>(subscriberGrainKey);
            Logger.LogInformation("{grainId} Added subscriber {subscriberKey} (total: {count})",
                GrainId, subscriberGrainKey, _subscriberLeases.Count);
        }
        else
        {
            Logger.LogDebug("{grainId} Renewed lease for existing subscriber {subscriberKey}",
                GrainId, subscriberGrainKey);
        }

        // Start timers if this is the first subscriber
        if (wasEmpty)
        {
            // Start lease check timer
            _leaseCheckTimer?.Dispose();
            _leaseCheckTimer = RegisterTimer(CheckExpiredLeases, null!, LeaseCheckInterval, LeaseCheckInterval);

            // Notify derived class to start polling
            OnFirstSubscriber();

            Logger.LogInformation("{grainId} Started polling (first subscriber)", GrainId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task Activate(IBarSubscriber subscriber)
    {
        // Get the subscriber's grain key from the reference
        var subscriberGrainKey = subscriber.GetPrimaryKeyString();

        var isNewSubscriber = !_subscriberLeases.ContainsKey(subscriberGrainKey);
        var wasEmpty = _subscriberLeases.Count == 0;

        // Set or renew the lease
        _subscriberLeases[subscriberGrainKey] = DateTime.UtcNow.Add(LeaseTimeout);

        // Cache the subscriber reference directly (avoids ambiguity with GetGrain<IBarSubscriber>)
        if (isNewSubscriber)
        {
            _subscribers[subscriberGrainKey] = subscriber;
            Logger.LogInformation("{grainId} Added subscriber {subscriberKey} via reference (total: {count})",
                GrainId, subscriberGrainKey, _subscriberLeases.Count);
        }
        else
        {
            Logger.LogDebug("{grainId} Renewed lease for existing subscriber {subscriberKey}",
                GrainId, subscriberGrainKey);
        }

        // Start timers if this is the first subscriber
        if (wasEmpty)
        {
            // Start lease check timer
            _leaseCheckTimer?.Dispose();
            _leaseCheckTimer = RegisterTimer(CheckExpiredLeases, null!, LeaseCheckInterval, LeaseCheckInterval);

            // Notify derived class to start polling
            OnFirstSubscriber();

            Logger.LogInformation("{grainId} Started polling (first subscriber)", GrainId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task Heartbeat(string subscriberGrainKey)
    {
        if (_subscriberLeases.ContainsKey(subscriberGrainKey))
        {
            _subscriberLeases[subscriberGrainKey] = DateTime.UtcNow.Add(LeaseTimeout);
            Logger.LogTrace("{grainId} Heartbeat from {subscriberKey}, lease renewed",
                GrainId, subscriberGrainKey);
        }
        else
        {
            Logger.LogWarning("{grainId} Heartbeat from unknown subscriber {subscriberKey}, treating as Activate",
                GrainId, subscriberGrainKey);
            return Activate(subscriberGrainKey);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task Deactivate()
    {
        Logger.LogInformation("{grainId} Deactivating, clearing all subscribers and stopping polling",
            GrainId);

        _subscriberLeases.Clear();
        _subscribers.Clear();

        await StopPollingInternal();
    }

    /// <inheritdoc/>
    public Task<bool> IsActive() => Task.FromResult(_subscriberLeases.Count > 0);

    /// <inheritdoc/>
    public abstract Task RetrieveBars();

    #endregion

    #region Lease Management

    /// <summary>
    /// Timer callback that checks for and removes expired subscriber leases.
    /// Deactivates the scraper if no subscribers remain.
    /// </summary>
    private async Task CheckExpiredLeases(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredSubscribers = _subscriberLeases
            .Where(kvp => kvp.Value < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var subscriberKey in expiredSubscribers)
        {
            _subscriberLeases.Remove(subscriberKey);
            _subscribers.Remove(subscriberKey);
            Logger.LogWarning("{grainId} Subscriber {subscriberKey} lease expired, removed",
                GrainId, subscriberKey);
        }

        // If no subscribers remain, stop polling
        if (_subscriberLeases.Count == 0)
        {
            Logger.LogInformation("{grainId} No subscribers remain, stopping polling", GrainId);
            await StopPollingInternal();
        }
    }

    /// <summary>
    /// Stops all polling and cleans up timers.
    /// </summary>
    private async Task StopPollingInternal()
    {
        _leaseCheckTimer?.Dispose();
        _leaseCheckTimer = null;

        await OnLastSubscriberRemoved();
    }

    #endregion

    #region Bar Delivery

    /// <summary>
    /// Delivers bars to all subscriber grains.
    /// Call this from derived classes after retrieving bars from the exchange.
    /// </summary>
    /// <param name="bars">The confirmed bars to deliver.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method:
    /// - Filters for confirmed bars only
    /// - Delivers to all subscribers
    /// - Removes failed subscribers automatically
    /// - Stops polling if all subscribers fail
    /// </remarks>
    protected async Task DeliverBarsToSubscribers(IEnumerable<BarEnvelope> bars)
    {
        if (_subscribers.Count == 0)
        {
            return;
        }

        // Filter for confirmed bars only for direct delivery
        var confirmedBars = bars.Where(b => b.Status.HasFlag(BarStatus.Confirmed)).ToArray();
        if (confirmedBars.Length == 0)
        {
            return;
        }

        // Deliver to all subscribers, collect any that fail
        var failedSubscribers = new List<string>();

        foreach (var (subscriberKey, subscriber) in _subscribers)
        {
            try
            {
                Logger.LogTrace("{grainId} Delivering {count} bars to {subscriber}",
                    GrainId, confirmedBars.Length, subscriberKey);

                await subscriber.OnBars(confirmedBars);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{grainId} Failed to deliver bars to subscriber {subscriber}",
                    GrainId, subscriberKey);
                failedSubscribers.Add(subscriberKey);
            }
        }

        // Remove failed subscribers
        foreach (var subscriberKey in failedSubscribers)
        {
            _subscriberLeases.Remove(subscriberKey);
            _subscribers.Remove(subscriberKey);
            Logger.LogWarning("{grainId} Removed failed subscriber {subscriberKey}",
                GrainId, subscriberKey);
        }

        // Stop polling if no subscribers remain
        if (_subscriberLeases.Count == 0)
        {
            Logger.LogInformation("{grainId} All subscribers failed, stopping polling", GrainId);
            await StopPollingInternal();
        }
    }

    #endregion
}
