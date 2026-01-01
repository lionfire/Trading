using System.ComponentModel;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Defines the operational mode for a trading account in a real-time bot harness.
/// </summary>
/// <remarks>
/// This enum distinguishes between different account types and their behavior:
/// <list type="bullet">
///   <item><description>Live modes use real-time market data with varying degrees of simulation</description></item>
///   <item><description>LiveReal is reserved for future integration with actual exchange accounts</description></item>
/// </list>
/// </remarks>
public enum BotAccountMode
{
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
