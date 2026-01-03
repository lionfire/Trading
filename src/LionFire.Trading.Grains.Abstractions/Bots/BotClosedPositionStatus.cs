using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Represents a closed position for display in the trade history.
/// </summary>
[GenerateSerializer]
[Alias("bot-closed-position-status")]
public class BotClosedPositionStatus
{
    /// <summary>
    /// Unique identifier for this position.
    /// </summary>
    [Id(0)]
    public int PositionId { get; set; }

    /// <summary>
    /// Trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [Id(1)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Position direction: "Long" or "Short".
    /// </summary>
    [Id(2)]
    public string Direction { get; set; } = "Long";

    /// <summary>
    /// Position size/quantity.
    /// </summary>
    [Id(3)]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Average entry price.
    /// </summary>
    [Id(4)]
    public decimal EntryPrice { get; set; }

    /// <summary>
    /// Time the position was opened.
    /// </summary>
    [Id(5)]
    public DateTime EntryTime { get; set; }

    /// <summary>
    /// Exit/close price.
    /// </summary>
    [Id(6)]
    public decimal ExitPrice { get; set; }

    /// <summary>
    /// Time the position was closed.
    /// </summary>
    [Id(7)]
    public DateTime ExitTime { get; set; }

    /// <summary>
    /// Realized profit/loss.
    /// </summary>
    [Id(8)]
    public decimal RealizedPnL { get; set; }

    /// <summary>
    /// Total commissions paid.
    /// </summary>
    [Id(9)]
    public decimal Commissions { get; set; }

    /// <summary>
    /// Total swap/funding fees (positive = received, negative = paid).
    /// </summary>
    [Id(10)]
    public decimal Swap { get; set; }

    /// <summary>
    /// Reason for closing: Manual, StopLoss, TakeProfit, Liquidation, Signal.
    /// </summary>
    [Id(11)]
    public string CloseReason { get; set; } = "Manual";

    /// <summary>
    /// User-defined label for the position.
    /// </summary>
    [Id(12)]
    public string? Label { get; set; }
}
