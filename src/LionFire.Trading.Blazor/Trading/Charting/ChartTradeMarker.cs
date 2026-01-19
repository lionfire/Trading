using System.Drawing;

namespace LionFire.Trading.Charting;

/// <summary>
/// Represents a trade marker to be displayed on a chart.
/// </summary>
public class ChartTradeMarker
{
    /// <summary>
    /// The time at which the trade occurred (must be on a bar boundary for best results).
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// True for buy/long entry, false for sell/short entry or exit.
    /// </summary>
    public bool IsBuy { get; set; }

    /// <summary>
    /// Optional unique identifier for this marker.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Optional text label to display on the marker (e.g., "Entry", "Exit", price, etc.).
    /// Defaults to "Buy" or "Sell" based on IsBuy.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Optional custom color. Defaults to green for buy, red for sell.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Optional size multiplier (default is 1.0).
    /// </summary>
    public double? Size { get; set; }

    /// <summary>
    /// Create a buy marker at the specified time.
    /// </summary>
    public static ChartTradeMarker Buy(DateTime time, string? text = null, string? id = null)
        => new() { Time = time, IsBuy = true, Text = text, Id = id };

    /// <summary>
    /// Create a sell marker at the specified time.
    /// </summary>
    public static ChartTradeMarker Sell(DateTime time, string? text = null, string? id = null)
        => new() { Time = time, IsBuy = false, Text = text, Id = id };

    /// <summary>
    /// Create a long entry marker.
    /// </summary>
    public static ChartTradeMarker LongEntry(DateTime time, string? text = null, string? id = null)
        => new() { Time = time, IsBuy = true, Text = text ?? "Long", Id = id };

    /// <summary>
    /// Create a long exit marker.
    /// </summary>
    public static ChartTradeMarker LongExit(DateTime time, string? text = null, string? id = null)
        => new() { Time = time, IsBuy = false, Text = text ?? "Close", Id = id, Color = System.Drawing.Color.Orange };

    /// <summary>
    /// Create a short entry marker.
    /// </summary>
    public static ChartTradeMarker ShortEntry(DateTime time, string? text = null, string? id = null)
        => new() { Time = time, IsBuy = false, Text = text ?? "Short", Id = id };

    /// <summary>
    /// Create a short exit marker.
    /// </summary>
    public static ChartTradeMarker ShortExit(DateTime time, string? text = null, string? id = null)
        => new() { Time = time, IsBuy = true, Text = text ?? "Close", Id = id, Color = System.Drawing.Color.Orange };
}
