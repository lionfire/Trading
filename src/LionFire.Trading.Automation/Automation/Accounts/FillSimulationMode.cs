using System.ComponentModel;

namespace LionFire.Trading.Automation.Accounts;

/// <summary>
/// Defines the mode for simulating order fills in live simulated trading.
/// </summary>
public enum FillSimulationMode
{
    /// <summary>
    /// Simple fill simulation - fills at current market price.
    /// </summary>
    /// <remarks>
    /// For stop orders: fills at current bid (for longs) or ask (for shorts).
    /// For limit orders: fills at the limit price.
    /// No slippage is applied.
    /// </remarks>
    [Description("Simple - Fill at market price")]
    Simple = 0,

    /// <summary>
    /// Realistic fill simulation with slippage and order book depth.
    /// </summary>
    /// <remarks>
    /// Applies configurable slippage based on:
    /// <list type="bullet">
    ///   <item><description>Order size relative to typical volume</description></item>
    ///   <item><description>Order book depth (if L2 data available)</description></item>
    ///   <item><description>Configured slippage basis points</description></item>
    /// </list>
    /// May result in partial fills for large orders.
    /// </remarks>
    [Description("Realistic - Slippage and order book depth")]
    Realistic = 1
}
