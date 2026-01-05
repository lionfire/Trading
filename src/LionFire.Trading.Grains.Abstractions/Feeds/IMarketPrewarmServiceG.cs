namespace LionFire.Trading.Feeds;

/// <summary>
/// Orleans grain interface for a service that pre-warms priority market data grains.
/// Uses Orleans reminders to periodically call EnsureActive on priority markets,
/// ensuring scrapers stay active for important symbols even without user subscribers.
/// </summary>
/// <remarks>
/// <para>
/// This is a singleton service grain (key: "0") that manages the list of priority
/// symbols to keep warm. It integrates with the demand-driven bar scraping architecture
/// by periodically calling EnsureActive on LastBarsG grains.
/// </para>
/// <para>
/// The reminder-based approach ensures the service survives silo restarts and continues
/// warming priority markets automatically.
/// </para>
/// </remarks>
public interface IMarketPrewarmServiceG : IGrainWithStringKey
{
    /// <summary>
    /// Starts the prewarm service, registering reminders to periodically warm priority markets.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Start();

    /// <summary>
    /// Stops the prewarm service, unregistering reminders.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Stop();

    /// <summary>
    /// Returns whether the prewarm service is currently running.
    /// </summary>
    /// <returns>True if the service is running, false otherwise.</returns>
    Task<bool> IsRunning();

    /// <summary>
    /// Gets the list of priority market grain IDs that are being kept warm.
    /// </summary>
    /// <returns>A list of LastBarsG grain keys (e.g., "binance.futures:BTCUSDT m1").</returns>
    Task<IReadOnlyList<string>> GetPriorityMarkets();

    /// <summary>
    /// Sets the list of priority market grain IDs to keep warm.
    /// </summary>
    /// <param name="marketGrainIds">LastBarsG grain keys to keep warm.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetPriorityMarkets(IReadOnlyList<string> marketGrainIds);

    /// <summary>
    /// Adds a market to the priority list.
    /// </summary>
    /// <param name="marketGrainId">The LastBarsG grain key to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddPriorityMarket(string marketGrainId);

    /// <summary>
    /// Removes a market from the priority list.
    /// </summary>
    /// <param name="marketGrainId">The LastBarsG grain key to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemovePriorityMarket(string marketGrainId);

    /// <summary>
    /// Manually triggers a prewarm cycle for all priority markets.
    /// Useful for testing or immediate warmup after adding new markets.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WarmNow();
}
