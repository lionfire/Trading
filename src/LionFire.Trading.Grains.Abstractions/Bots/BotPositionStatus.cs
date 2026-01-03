using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Represents an open position for display in the trading dashboard.
/// </summary>
[GenerateSerializer]
[Alias("bot-position-status")]
public class BotPositionStatus
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
    /// Current market price.
    /// </summary>
    [Id(6)]
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Mark price (for futures/perpetuals).
    /// </summary>
    [Id(7)]
    public decimal? MarkPrice { get; set; }

    /// <summary>
    /// Liquidation price (for leveraged positions).
    /// </summary>
    [Id(8)]
    public decimal? LiqPrice { get; set; }

    /// <summary>
    /// Gross profit/loss before fees.
    /// </summary>
    [Id(9)]
    public decimal GrossProfit { get; set; }

    /// <summary>
    /// Net profit/loss after fees.
    /// </summary>
    [Id(10)]
    public decimal NetProfit { get; set; }

    /// <summary>
    /// Total commissions paid.
    /// </summary>
    [Id(11)]
    public decimal Commissions { get; set; }

    /// <summary>
    /// Swap/funding fees (positive = received, negative = paid).
    /// </summary>
    [Id(12)]
    public decimal Swap { get; set; }

    /// <summary>
    /// Stop loss price, if set.
    /// </summary>
    [Id(13)]
    public decimal? StopLoss { get; set; }

    /// <summary>
    /// Take profit price, if set.
    /// </summary>
    [Id(14)]
    public decimal? TakeProfit { get; set; }

    /// <summary>
    /// User-defined label for the position.
    /// </summary>
    [Id(15)]
    public string? Label { get; set; }
}
