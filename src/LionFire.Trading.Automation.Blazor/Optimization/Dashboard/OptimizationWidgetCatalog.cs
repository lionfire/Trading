using LionFire.Trading.Automation.Blazor.Optimization.Dashboard.Widgets;
using MudBlazor;

namespace LionFire.Trading.Automation.Blazor.Optimization.Dashboard;

/// <summary>
/// Registry of available widgets for optimization dashboards.
/// </summary>
public static class OptimizationWidgetCatalog
{
    public static readonly OptimizationWidgetInfo[] Widgets = new[]
    {
        new OptimizationWidgetInfo(
            Id: "histogram",
            DisplayName: "Statistics Histogram",
            ComponentType: typeof(StatisticsHistogramWidget),
            DefaultWidth: 6,
            DefaultHeight: 5,
            Icon: Icons.Material.Filled.BarChart,
            Description: "Distribution chart showing metric histograms with configurable bucket sizes",
            MinWidth: 4,
            MinHeight: 4),

        new OptimizationWidgetInfo(
            Id: "equity-curves",
            DisplayName: "Equity Curves",
            ComponentType: typeof(EquityCurvesWidget),
            DefaultWidth: 8,
            DefaultHeight: 4,
            Icon: Icons.Material.Filled.ShowChart,
            Description: "Backtest equity curve chart showing selected results",
            MinWidth: 4,
            MinHeight: 3),

        new OptimizationWidgetInfo(
            Id: "filter",
            DisplayName: "Results Filter",
            ComponentType: typeof(ResultsFilterWidget),
            DefaultWidth: 4,
            DefaultHeight: 4,
            Icon: Icons.Material.Filled.FilterList,
            Description: "Filter controls for optimization results",
            MinWidth: 3,
            MinHeight: 3),

        new OptimizationWidgetInfo(
            Id: "data-grid",
            DisplayName: "Backtest Results",
            ComponentType: typeof(BacktestDataGridWidget),
            DefaultWidth: 12,
            DefaultHeight: 5,
            Icon: Icons.Material.Filled.TableChart,
            Description: "Sortable and filterable results table",
            MinWidth: 6,
            MinHeight: 3),

        new OptimizationWidgetInfo(
            Id: "summary",
            DisplayName: "Results Summary",
            ComponentType: typeof(ResultsSummaryWidget),
            DefaultWidth: 4,
            DefaultHeight: 5,
            Icon: Icons.Material.Filled.Summarize,
            Description: "Summary statistics panel for optimization results",
            MinWidth: 3,
            MinHeight: 3),
    };

    /// <summary>
    /// Finds a widget info by its ID.
    /// </summary>
    public static OptimizationWidgetInfo? FindById(string id)
        => Widgets.FirstOrDefault(w => w.Id == id);
}

/// <summary>
/// Information about an available optimization widget.
/// </summary>
public record OptimizationWidgetInfo(
    string Id,
    string DisplayName,
    Type ComponentType,
    int DefaultWidth,
    int DefaultHeight,
    string Icon,
    string Description = "",
    int MinWidth = 3,
    int MinHeight = 3);
