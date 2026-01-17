using Orleans;

namespace LionFire.Trading.Grains.User;

/// <summary>
/// Preferences for column visibility and configuration in a data grid or table view.
/// </summary>
[GenerateSerializer]
[Alias("column-preferences")]
public class ColumnPreferences
{
    /// <summary>
    /// Dictionary of column IDs to their visibility state.
    /// True = visible, False = hidden.
    /// Columns not present in this dictionary use their default visibility.
    /// </summary>
    [Id(0)]
    public Dictionary<string, bool> ColumnVisibility { get; set; } = new();

    /// <summary>
    /// Optional: Column order (list of column IDs in display order).
    /// Null means use default order.
    /// </summary>
    [Id(1)]
    public List<string>? ColumnOrder { get; set; }

    /// <summary>
    /// Optional: Column widths (column ID to width in pixels or percentage).
    /// Null means use default widths.
    /// </summary>
    [Id(2)]
    public Dictionary<string, int>? ColumnWidths { get; set; }

    /// <summary>
    /// Timestamp when preferences were last modified.
    /// </summary>
    [Id(3)]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
