using Microsoft.Extensions.Logging;
using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Grain interface for a realtime bot harness that manages the lifecycle of a trading bot
/// in a live market environment with real-time data feeds.
/// </summary>
/// <remarks>
/// <para>
/// <b>Current Status: Option A - Signal/Alert Bots Only</b>
/// Real-time bots currently run WITHOUT a BotContext assigned, which works for
/// signal generation and monitoring bots that don't need to place orders.
/// </para>
/// <para>
/// <b>Future: Option B - Trading Bots with Account Integration</b>
/// For bots that need to place orders and manage positions in real-time.
/// </para>
/// <para>
/// This grain is bot-centric (uses IGrainWithStringKey) allowing multiple markets per bot
/// and multiple bots per market. The grain ID is typically "bot123" format, NOT market-based.
/// </para>
/// <para>
/// The harness manages:
/// - Subscribing to real-time market data channels
/// - Catching up on historical bars when starting
/// - Processing real-time bar updates
/// - Bot lifecycle state management
/// </para>
/// </remarks>
public interface IRealtimeBotHarnessG : IGrainWithStringKey
{
    /// <summary>
    /// Starts the realtime bot harness with the specified configuration.
    /// </summary>
    /// <param name="config">Configuration specifying bot type, account, markets, and parameters</param>
    /// <returns>True if successfully started, false if already running or configuration is invalid</returns>
    /// <remarks>
    /// Starting the harness will:
    /// 1. Validate the configuration
    /// 2. Subscribe to market data channels for all configured markets
    /// 3. Catch up on historical bars from the last saved position
    /// 4. Transition to real-time processing when caught up
    /// </remarks>
    ValueTask<bool> Start(RealtimeBotConfiguration config);

    /// <summary>
    /// Stops the realtime bot harness and unsubscribes from all market data channels.
    /// </summary>
    /// <remarks>
    /// Stopping the harness will:
    /// 1. Unsubscribe from all market data channels
    /// 2. Save the current position (last bar time)
    /// 3. Dispose of the bot instance
    /// 4. Transition state to Stopped
    /// </remarks>
    ValueTask Stop();

    /// <summary>
    /// Gets the current status of the realtime bot harness.
    /// </summary>
    /// <returns>Status information including state, last bar time, and any error messages</returns>
    ValueTask<RealtimeBotStatus> GetStatus();

    #region Logging

    /// <summary>
    /// Gets recent logs from the in-memory circular buffer.
    /// </summary>
    /// <param name="count">Maximum number of entries to return (default: 100)</param>
    /// <param name="minLevel">Minimum log level filter</param>
    /// <returns>Recent log entries, newest last</returns>
    ValueTask<List<BotLogEntry>> GetRecentLogs(int count = 100, LogLevel minLevel = LogLevel.Trace);

    /// <summary>
    /// Gets log entries since a specific timestamp (for polling-based updates).
    /// </summary>
    /// <param name="since">Return logs after this timestamp</param>
    /// <returns>Log entries created after the specified time</returns>
    ValueTask<List<BotLogEntry>> GetLogsSince(DateTime since);

    /// <summary>
    /// Gets historical session metadata.
    /// </summary>
    /// <returns>List of past sessions with metadata (from current grain activation)</returns>
    ValueTask<List<BotHarnessSession>> GetSessions();

    /// <summary>
    /// Subscribes an observer for real-time log updates.
    /// </summary>
    /// <param name="observer">The observer to receive log updates</param>
    ValueTask SubscribeToLogs(IBotLogObserver observer);

    /// <summary>
    /// Unsubscribes an observer from log updates.
    /// </summary>
    /// <param name="observer">The observer to remove</param>
    ValueTask UnsubscribeFromLogs(IBotLogObserver observer);

    /// <summary>
    /// Updates the configured log level for this harness.
    /// </summary>
    /// <param name="level">New minimum log level, or null to use system default</param>
    ValueTask SetLogLevel(LogLevel? level);

    #endregion

    #region Account

    /// <summary>
    /// Gets the current account state including balance and equity.
    /// </summary>
    /// <returns>Account status with balance, equity, and P&L information, or null if no account is configured or bot is not running.</returns>
    ValueTask<BotAccountStatus?> GetAccountStatus();

    #endregion

    #region Positions and Orders

    /// <summary>
    /// Gets all open positions for the bot's account.
    /// </summary>
    /// <returns>List of open positions, or empty list if no account is configured or bot is not running.</returns>
    ValueTask<List<BotPositionStatus>> GetOpenPositions();

    /// <summary>
    /// Gets closed positions from the current session.
    /// </summary>
    /// <param name="limit">Maximum number of positions to return (newest first)</param>
    /// <returns>List of closed positions, or empty list if no account is configured or bot is not running.</returns>
    ValueTask<List<BotClosedPositionStatus>> GetClosedPositions(int limit = 100);

    /// <summary>
    /// Gets all open orders (pending, stop loss, take profit).
    /// </summary>
    /// <returns>List of open orders, or empty list if no account is configured or bot is not running.</returns>
    ValueTask<List<BotOrderStatus>> GetOpenOrders();

    #endregion
}
