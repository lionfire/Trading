using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace LionFire.Trading.Automation.Blazor.Optimization.Dashboard;

/// <summary>
/// Runtime instance of a widget in an optimization dashboard.
/// </summary>
public class OptimizationWidgetInstance
{
    /// <summary>
    /// Unique identifier for this widget instance.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display title of the widget.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The widget type ID from the catalog.
    /// </summary>
    public required string WidgetTypeId { get; init; }

    /// <summary>
    /// Component type to render.
    /// </summary>
    public required Type ComponentType { get; init; }

    /// <summary>
    /// X position in the grid (column).
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y position in the grid (row).
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Width in grid units.
    /// </summary>
    public int W { get; set; } = 4;

    /// <summary>
    /// Height in grid units.
    /// </summary>
    public int H { get; set; } = 3;

    /// <summary>
    /// Minimum width in grid units.
    /// </summary>
    public int MinW { get; set; } = 2;

    /// <summary>
    /// Minimum height in grid units.
    /// </summary>
    public int MinH { get; set; } = 2;

    /// <summary>
    /// Widget-specific settings.
    /// </summary>
    public Dictionary<string, object>? Settings { get; set; }

    /// <summary>
    /// Error boundary for this widget.
    /// </summary>
    public ErrorBoundary? ErrorBoundary { get; set; }

    /// <summary>
    /// Creates a widget instance from a catalog definition.
    /// </summary>
    public static OptimizationWidgetInstance FromCatalog(OptimizationWidgetInfo info, int x = 0, int y = 0)
    {
        return new OptimizationWidgetInstance
        {
            Id = Guid.NewGuid().ToString(),
            Title = info.DisplayName,
            WidgetTypeId = info.Id,
            ComponentType = info.ComponentType,
            X = x,
            Y = y,
            W = info.DefaultWidth,
            H = info.DefaultHeight,
            MinW = 2,
            MinH = 2
        };
    }
}
