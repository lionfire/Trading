using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Represents the current account status of a trading bot, including balance and profit/loss information.
/// </summary>
[GenerateSerializer]
[Alias("bot-account-status")]
public class BotAccountStatus
{
    /// <summary>
    /// Current account balance (available funds).
    /// </summary>
    [Id(0)]
    public decimal Balance { get; set; }

    /// <summary>
    /// Current account equity (balance + unrealized P&L).
    /// </summary>
    [Id(1)]
    public decimal Equity { get; set; }

    /// <summary>
    /// Unrealized profit/loss from open positions.
    /// </summary>
    [Id(2)]
    public decimal UnrealizedPnL { get; set; }

    /// <summary>
    /// Realized profit/loss from closed positions.
    /// </summary>
    [Id(3)]
    public decimal RealizedPnL { get; set; }

    /// <summary>
    /// Base currency for the account (e.g., "USDT", "USD").
    /// </summary>
    [Id(4)]
    public string BaseCurrency { get; set; } = "USDT";

    /// <summary>
    /// Account mode (LiveSimulated, LivePaper, LiveReal).
    /// </summary>
    [Id(5)]
    public BotAccountMode Mode { get; set; }
}
