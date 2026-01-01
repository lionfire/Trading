using System.ComponentModel;

namespace LionFire.Trading.Automation.Accounts;

/// <summary>
/// Defines the operational mode for a trading account.
/// </summary>
/// <remarks>
/// This enum distinguishes between different account types and their behavior:
/// <list type="bullet">
///   <item><description>Backtest mode uses historical data with simulated execution</description></item>
///   <item><description>Live modes use real-time market data with varying degrees of simulation</description></item>
///   <item><description>LiveReal is reserved for future integration with actual exchange accounts</description></item>
/// </list>
/// </remarks>
public enum BotAccountMode
{
    /// <summary>
    /// Historical backtesting mode.
    /// </summary>
    /// <remarks>
    /// Uses historical market data and <see cref="SimAccount{TPrecision}"/> for order execution.
    /// Time is simulated and can be advanced much faster than real-time.
    /// </remarks>
    [Description("Backtest - Historical simulation")]
    Backtest = 0,

    /// <summary>
    /// Live trading with realistic balance tracking.
    /// </summary>
    /// <remarks>
    /// Uses real-time market data with simulated order execution.
    /// Balance starts at a configured amount (default $10,000 USD equivalent) and
    /// changes based on realized P&amp;L. The account can "blow up" if losses exceed balance.
    /// </remarks>
    [Description("Live Simulated - Realistic balance tracking")]
    LiveSimulated = 1,

    /// <summary>
    /// Live trading with infinite capital (paper trading).
    /// </summary>
    /// <remarks>
    /// Uses real-time market data with simulated order execution.
    /// Balance and equity always return the configured amount regardless of trading activity.
    /// Positions are tracked but balance never changes. Useful for testing strategies
    /// without capital constraints.
    /// </remarks>
    [Description("Live Paper - Infinite capital mode")]
    LivePaper = 2,

    /// <summary>
    /// Live trading with real exchange integration.
    /// </summary>
    /// <remarks>
    /// Reserved for future implementation. Will integrate with actual exchange APIs
    /// for real order execution with real funds.
    /// </remarks>
    [Description("Live Real - Real exchange trading")]
    LiveReal = 3
}

/// <summary>
/// Extension methods for <see cref="BotAccountMode"/>.
/// </summary>
public static class BotAccountModeExtensions
{
    /// <summary>
    /// Determines whether the account mode uses live (real-time) market data.
    /// </summary>
    /// <param name="mode">The account mode to check.</param>
    /// <returns>
    /// <c>true</c> for <see cref="BotAccountMode.LiveSimulated"/>,
    /// <see cref="BotAccountMode.LivePaper"/>, and <see cref="BotAccountMode.LiveReal"/>;
    /// <c>false</c> for <see cref="BotAccountMode.Backtest"/>.
    /// </returns>
    public static bool IsLive(this BotAccountMode mode) => mode != BotAccountMode.Backtest;

    /// <summary>
    /// Determines whether the account mode uses simulated order execution.
    /// </summary>
    /// <param name="mode">The account mode to check.</param>
    /// <returns>
    /// <c>true</c> for <see cref="BotAccountMode.Backtest"/>,
    /// <see cref="BotAccountMode.LiveSimulated"/>, and <see cref="BotAccountMode.LivePaper"/>;
    /// <c>false</c> for <see cref="BotAccountMode.LiveReal"/>.
    /// </returns>
    public static bool IsSimulated(this BotAccountMode mode) => mode != BotAccountMode.LiveReal;

    /// <summary>
    /// Determines whether the account mode uses real exchange trading.
    /// </summary>
    /// <param name="mode">The account mode to check.</param>
    /// <returns>
    /// <c>true</c> only for <see cref="BotAccountMode.LiveReal"/>;
    /// <c>false</c> for all simulated modes.
    /// </returns>
    public static bool IsReal(this BotAccountMode mode) => mode == BotAccountMode.LiveReal;

    /// <summary>
    /// Determines whether the account mode tracks realistic balance (can blow up).
    /// </summary>
    /// <param name="mode">The account mode to check.</param>
    /// <returns>
    /// <c>true</c> for <see cref="BotAccountMode.Backtest"/>,
    /// <see cref="BotAccountMode.LiveSimulated"/>, and <see cref="BotAccountMode.LiveReal"/>;
    /// <c>false</c> for <see cref="BotAccountMode.LivePaper"/> (infinite capital).
    /// </returns>
    public static bool HasRealisticBalance(this BotAccountMode mode) => mode != BotAccountMode.LivePaper;
}
