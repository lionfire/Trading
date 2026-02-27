using LionFire.Trading.Grains.Bots;
using Orleans;

namespace LionFire.Trading.Grains.Signals;

/// <summary>
/// Orleans grain that collects and aggregates signals from multiple bot harnesses
/// for a single symbol. Keyed by symbol (e.g., "BTCUSDT").
/// </summary>
/// <remarks>
/// <para>
/// The collector subscribes as an <see cref="IBotSignalObserver"/> to each registered
/// bot harness, receives <see cref="BotSignalEvent"/> notifications, and feeds them
/// into the signal-aggregator engine to produce consensus signals.
/// </para>
/// <para>
/// UI components subscribe via <see cref="IAggregatedSignalObserver"/> to receive
/// real-time aggregated signal updates.
/// </para>
/// </remarks>
public interface ISignalCollectorG : IGrainWithStringKey
{
    /// <summary>
    /// Registers a bot harness to be monitored for signals.
    /// The collector will subscribe as an <see cref="IBotSignalObserver"/> to the harness.
    /// </summary>
    /// <param name="harnessGrainId">The grain ID of the <see cref="IRealtimeBotHarnessG"/>.</param>
    /// <param name="metadata">Optional deployment metadata for signal enrichment.</param>
    ValueTask RegisterBotHarness(string harnessGrainId, BotDeploymentMetadata? metadata = null);

    /// <summary>
    /// Unregisters a bot harness and stops monitoring its signals.
    /// </summary>
    /// <param name="harnessGrainId">The grain ID of the bot harness to remove.</param>
    ValueTask UnregisterBotHarness(string harnessGrainId);

    /// <summary>
    /// Gets the current aggregated signal snapshot.
    /// </summary>
    ValueTask<AggregatedSignalSnapshot> GetAggregatedSignal();

    /// <summary>
    /// Gets the list of registered bot harness IDs.
    /// </summary>
    ValueTask<List<string>> GetRegisteredBots();

    /// <summary>
    /// Subscribes for real-time aggregated signal updates.
    /// </summary>
    ValueTask SubscribeToAggregatedSignals(IAggregatedSignalObserver observer);

    /// <summary>
    /// Unsubscribes from aggregated signal updates.
    /// </summary>
    ValueTask UnsubscribeFromAggregatedSignals(IAggregatedSignalObserver observer);

    /// <summary>
    /// Gets recent signal history for this symbol.
    /// </summary>
    /// <param name="count">Maximum number of signals to return (newest first).</param>
    ValueTask<List<BotSignalEvent>> GetRecentSignals(int count = 50);
}
