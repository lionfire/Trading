using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Represents an open/pending order for display in the trading dashboard.
/// Note: For simulated/paper trading, SL/TP from positions are represented as pending orders.
/// </summary>
[GenerateSerializer]
[Alias("bot-order-status")]
public class BotOrderStatus
{
    /// <summary>
    /// Unique identifier for this order.
    /// </summary>
    [Id(0)]
    public int OrderId { get; set; }

    /// <summary>
    /// Trading symbol (e.g., "BTCUSDT").
    /// </summary>
    [Id(1)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Order type: Limit, Stop, StopLoss, TakeProfit, StopLimit.
    /// </summary>
    [Id(2)]
    public string OrderType { get; set; } = "Limit";

    /// <summary>
    /// Order side: Buy or Sell.
    /// </summary>
    [Id(3)]
    public string Side { get; set; } = "Buy";

    /// <summary>
    /// Order quantity.
    /// </summary>
    [Id(4)]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Trigger price for stop/SL/TP orders.
    /// </summary>
    [Id(5)]
    public decimal TriggerPrice { get; set; }

    /// <summary>
    /// Limit price for limit/stop-limit orders.
    /// </summary>
    [Id(6)]
    public decimal? LimitPrice { get; set; }

    /// <summary>
    /// Time the order was created.
    /// </summary>
    [Id(7)]
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Order status: Open, PartiallyFilled, Cancelled.
    /// </summary>
    [Id(8)]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// Amount already filled.
    /// </summary>
    [Id(9)]
    public decimal FilledQuantity { get; set; }

    /// <summary>
    /// Position ID this order is linked to (for SL/TP orders).
    /// </summary>
    [Id(10)]
    public int? LinkedPositionId { get; set; }

    /// <summary>
    /// User-defined label.
    /// </summary>
    [Id(11)]
    public string? Label { get; set; }
}
