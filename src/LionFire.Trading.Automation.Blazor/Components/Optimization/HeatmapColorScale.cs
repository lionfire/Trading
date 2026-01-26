namespace LionFire.Trading.Automation.Blazor.Components.Optimization;

/// <summary>
/// Provides color mapping for AD (Annualized ROI / Drawdown) scores in heatmap visualizations.
/// </summary>
public static class HeatmapColorScale
{
    /// <summary>
    /// Color thresholds for AD scores.
    /// </summary>
    public static class Thresholds
    {
        public const double Poor = 0.5;
        public const double Marginal = 1.0;
        public const double Good = 2.0;
        public const double Excellent = 3.0;
    }

    /// <summary>
    /// Background colors for each score range.
    /// </summary>
    public static class Colors
    {
        public const string Poor = "#FF6B6B";       // Red - AD < 0.5
        public const string Marginal = "#FFE66D";   // Yellow - AD 0.5-1.0
        public const string Good = "#4ECDC4";       // Teal - AD 1.0-2.0
        public const string Excellent = "#45B7AA";  // Green - AD 2.0-3.0
        public const string Outstanding = "#2ECC71"; // Bright Green - AD > 3.0
        public const string NoData = "#E0E0E0";     // Gray - null/no data
    }

    /// <summary>
    /// Gets the background color for an AD score.
    /// </summary>
    /// <param name="ad">The AD score, or null if no data.</param>
    /// <returns>CSS color string.</returns>
    public static string GetColor(double? ad)
    {
        if (!ad.HasValue) return Colors.NoData;

        return ad.Value switch
        {
            < Thresholds.Poor => Colors.Poor,
            < Thresholds.Marginal => Colors.Marginal,
            < Thresholds.Good => Colors.Good,
            < Thresholds.Excellent => Colors.Excellent,
            _ => Colors.Outstanding
        };
    }

    /// <summary>
    /// Gets the text color that provides good contrast for an AD score's background.
    /// </summary>
    /// <param name="ad">The AD score, or null if no data.</param>
    /// <returns>CSS color string for text.</returns>
    public static string GetTextColor(double? ad)
    {
        if (!ad.HasValue) return "#666666"; // Dark gray for no-data cells

        // Use dark text for light backgrounds (yellow), light text for dark backgrounds
        return ad.Value switch
        {
            < Thresholds.Poor => "#FFFFFF",     // White on red
            < Thresholds.Marginal => "#333333", // Dark on yellow
            < Thresholds.Good => "#FFFFFF",     // White on teal
            < Thresholds.Excellent => "#FFFFFF", // White on green
            _ => "#FFFFFF"                       // White on bright green
        };
    }

    /// <summary>
    /// Gets a human-readable label for an AD score range.
    /// </summary>
    /// <param name="ad">The AD score, or null if no data.</param>
    /// <returns>Label describing the score quality.</returns>
    public static string GetLabel(double? ad)
    {
        if (!ad.HasValue) return "No Data";

        return ad.Value switch
        {
            < Thresholds.Poor => "Poor",
            < Thresholds.Marginal => "Marginal",
            < Thresholds.Good => "Good",
            < Thresholds.Excellent => "Excellent",
            _ => "Outstanding"
        };
    }

    /// <summary>
    /// Gets all color scale entries for rendering a legend.
    /// </summary>
    /// <returns>List of color scale entries with ranges and colors.</returns>
    public static IReadOnlyList<ColorScaleEntry> GetLegendEntries() =>
    [
        new("< 0.5", Colors.Poor, "Poor"),
        new("0.5-1.0", Colors.Marginal, "Marginal"),
        new("1.0-2.0", Colors.Good, "Good"),
        new("2.0-3.0", Colors.Excellent, "Excellent"),
        new("> 3.0", Colors.Outstanding, "Outstanding"),
        new("N/A", Colors.NoData, "No Data")
    ];

    /// <summary>
    /// Represents an entry in the color scale legend.
    /// </summary>
    public record ColorScaleEntry(string Range, string Color, string Label);
}
