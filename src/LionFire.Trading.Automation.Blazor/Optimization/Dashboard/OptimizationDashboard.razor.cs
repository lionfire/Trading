using BlazorGridStack;
using BlazorGridStack.Models;
using LionFire.Trading.Workbench;
using LionFire.Trading.Workbench.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LionFire.Trading.Automation.Blazor.Optimization.Dashboard;

public class MakeWidgetResult
{
    public bool Success { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}

public partial class OptimizationDashboard : IAsyncDisposable
{
    [Parameter]
    public OneShotOptimizeVM? ViewModel { get; set; }

    [Parameter]
    public string DashboardName { get; set; } = "dashboard1";

    [Inject]
    private WorkbenchLayoutService LayoutService { get; set; } = default!;

    [Inject]
    private ILogger<OptimizationDashboard> Logger { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private readonly List<OptimizationWidgetInstance> _widgets = new();
    private bool _showWidgetPicker;
    private bool _disposed;
    private bool _isLoading = true;

    private BlazorGridStackBody? _grid;

    private BlazorGridStackBodyOptions _gridOptions = new()
    {
        Column = 12,
        CellHeight = "80",
        Margin = "4",
        Float = true,
        Animate = true,
        DisableOneColumnMode = true,
        Handle = ".widget-header"
    };

    private string LayoutKey => $"optimization:{DashboardName}";

    protected override async Task OnInitializedAsync()
    {
        await LoadLayoutAsync();
        _isLoading = false;
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_widgets.Any() && !_isLoading)
        {
            AddDefaultWidgets();
        }
        await base.OnParametersSetAsync();
    }

    private void ShowWidgetPicker()
    {
        _showWidgetPicker = true;
    }

    private string? _pendingWidgetId;

    private async Task AddWidget(OptimizationWidgetInfo info)
    {
        // Use autoPosition - let GridStack find the best spot
        var widget = OptimizationWidgetInstance.FromCatalog(info, 0, 0);
        widget.AutoPosition = true;
        _widgets.Add(widget);
        _showWidgetPicker = false;
        _pendingWidgetId = widget.Id;

        // StateHasChanged will trigger OnAfterRenderAsync where we'll make the widget
        // SaveLayoutAsync is called after makeWidget to get the correct position
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // If we have a pending widget, tell GridStack to make it interactive
        if (_pendingWidgetId != null)
        {
            var widgetId = _pendingWidgetId;
            _pendingWidgetId = null;

            // Small delay to ensure DOM is updated
            await Task.Delay(100);

            try
            {
                var result = await JS.InvokeAsync<MakeWidgetResult>("chartResizeObserver.makeWidget", widgetId);

                // Update widget with actual position GridStack assigned
                var widget = _widgets.FirstOrDefault(w => w.Id == widgetId);
                if (widget != null && result?.Success == true)
                {
                    widget.X = result.X;
                    widget.Y = result.Y;
                    widget.W = result.W;
                    widget.H = result.H;
                    widget.AutoPosition = false; // Clear flag after placement
                    await SaveLayoutAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to make widget interactive: {WidgetId}", widgetId);
            }
        }
    }

    private async Task RemoveWidget(string widgetId)
    {
        var widget = _widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget != null)
        {
            // Remove from GridStack first
            try
            {
                await JS.InvokeVoidAsync("chartResizeObserver.removeWidget", widgetId);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to remove widget from grid: {WidgetId}", widgetId);
            }

            _widgets.Remove(widget);
            await SaveLayoutAsync();
        }
    }

    private Dictionary<string, object> GetWidgetParameters(OptimizationWidgetInstance widget)
    {
        return new Dictionary<string, object>
        {
            { "ViewModel", ViewModel! },
            { "WidgetInstance", widget },
            { "OnSettingsChanged", EventCallback.Factory.Create<OptimizationWidgetInstance>(this, OnWidgetSettingsChanged) }
        };
    }

    private async Task OnWidgetSettingsChanged(OptimizationWidgetInstance widget)
    {
        await SaveLayoutAsync();
    }

    private async Task RefreshAsync()
    {
        StateHasChanged();
        await Task.CompletedTask;
    }

    #region Layout Persistence

    private async Task OnLayoutChanged(BlazorGridStackWidgetListEventArgs args)
    {
        if (args.Items != null)
        {
            foreach (var item in args.Items)
            {
                var widget = _widgets.FirstOrDefault(w => w.Id == item.Id);
                if (widget != null)
                {
                    widget.X = item.X;
                    widget.Y = item.Y;
                    widget.W = item.W;
                    widget.H = item.H;
                }
            }
        }

        await SaveLayoutAsync();

        // Trigger chart resize after layout change
        await ResizeChartsAsync();
    }

    private async Task ResizeChartsAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("chartResizeObserver.resizeAllCharts");
        }
        catch
        {
            // Ignore JS interop errors
        }
    }

    private async Task SaveLayoutAsync()
    {
        try
        {
            var layoutItems = _widgets.Select(w => new WidgetLayoutItem
            {
                Id = w.Id,
                Title = w.Title,
                ComponentTypeName = w.WidgetTypeId,
                X = w.X,
                Y = w.Y,
                W = w.W,
                H = w.H,
                MinW = w.MinW,
                MinH = w.MinH,
                Settings = w.Settings
            }).ToList();

            var layout = new WorkbenchLayout { Widgets = layoutItems };

            Logger.LogDebug("[OptimizationDashboard] Saving layout '{LayoutKey}' with {WidgetCount} items",
                LayoutKey, layout.Widgets.Count);
            await LayoutService.SaveLayoutAsync(LayoutKey, layout);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OptimizationDashboard] Failed to save layout '{LayoutKey}'", LayoutKey);
        }
    }

    private async Task LoadLayoutAsync()
    {
        try
        {
            Logger.LogDebug("[OptimizationDashboard] Loading layout '{LayoutKey}'", LayoutKey);
            var layout = await LayoutService.GetLayoutAsync(LayoutKey);

            if (layout?.Widgets != null && layout.Widgets.Count > 0)
            {
                Logger.LogInformation("[OptimizationDashboard] Loaded layout '{LayoutKey}' with {WidgetCount} items",
                    LayoutKey, layout.Widgets.Count);
                _widgets.Clear();

                foreach (var item in layout.Widgets)
                {
                    var widgetInfo = OptimizationWidgetCatalog.FindById(item.ComponentTypeName);
                    if (widgetInfo == null)
                    {
                        Logger.LogWarning("[OptimizationDashboard] Unknown widget type: {WidgetType}", item.ComponentTypeName);
                        continue;
                    }

                    _widgets.Add(new OptimizationWidgetInstance
                    {
                        Id = item.Id,
                        Title = item.Title,
                        WidgetTypeId = item.ComponentTypeName,
                        ComponentType = widgetInfo.ComponentType,
                        X = item.X,
                        Y = item.Y,
                        W = item.W,
                        H = item.H,
                        MinW = item.MinW,
                        MinH = item.MinH,
                        Settings = item.Settings
                    });
                }

                Logger.LogDebug("[OptimizationDashboard] Loaded {WidgetCount} widgets", _widgets.Count);
            }
            else
            {
                Logger.LogInformation("[OptimizationDashboard] No saved layout found for '{LayoutKey}', using defaults", LayoutKey);
                AddDefaultWidgets();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OptimizationDashboard] Failed to load layout '{LayoutKey}'", LayoutKey);
            AddDefaultWidgets();
        }
    }

    private void AddDefaultWidgets()
    {
        _widgets.Clear();

        if (DashboardName == "dashboard1")
        {
            // Dashboard 1 (Analysis Focus):
            // - Equity Curves (8x4 at 0,0)
            // - Filter (4x4 at 8,0)
            // - Data Grid (12x5 at 0,4)

            var equityCurves = OptimizationWidgetCatalog.FindById("equity-curves");
            if (equityCurves != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = equityCurves.DisplayName,
                    WidgetTypeId = equityCurves.Id,
                    ComponentType = equityCurves.ComponentType,
                    X = 0, Y = 0, W = 8, H = 4, MinW = 4, MinH = 3
                });
            }

            var filter = OptimizationWidgetCatalog.FindById("filter");
            if (filter != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = filter.DisplayName,
                    WidgetTypeId = filter.Id,
                    ComponentType = filter.ComponentType,
                    X = 8, Y = 0, W = 4, H = 4, MinW = 3, MinH = 3
                });
            }

            var dataGrid = OptimizationWidgetCatalog.FindById("data-grid");
            if (dataGrid != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = dataGrid.DisplayName,
                    WidgetTypeId = dataGrid.Id,
                    ComponentType = dataGrid.ComponentType,
                    X = 0, Y = 4, W = 12, H = 5, MinW = 6, MinH = 3
                });
            }
        }
        else if (DashboardName == "dashboard2")
        {
            // Dashboard 2 (Statistics Focus):
            // - Histogram AD (6x4 at 0,0)
            // - Histogram Fitness (6x4 at 6,0)
            // - Summary (4x4 at 0,4)
            // - Filter (4x4 at 4,4)
            // - Data Grid (4x4 at 8,4)

            var histogram = OptimizationWidgetCatalog.FindById("histogram");
            if (histogram != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Histogram (AD)",
                    WidgetTypeId = histogram.Id,
                    ComponentType = histogram.ComponentType,
                    X = 0, Y = 0, W = 6, H = 5, MinW = 4, MinH = 4,
                    Settings = new Dictionary<string, object> { { "SelectedMetric", "AD" } }
                });

                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Histogram (Fitness)",
                    WidgetTypeId = histogram.Id,
                    ComponentType = histogram.ComponentType,
                    X = 6, Y = 0, W = 6, H = 5, MinW = 4, MinH = 4,
                    Settings = new Dictionary<string, object> { { "SelectedMetric", "Fitness" } }
                });
            }

            var summary = OptimizationWidgetCatalog.FindById("summary");
            if (summary != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = summary.DisplayName,
                    WidgetTypeId = summary.Id,
                    ComponentType = summary.ComponentType,
                    X = 0, Y = 5, W = 4, H = 5, MinW = 3, MinH = 3
                });
            }

            var filter = OptimizationWidgetCatalog.FindById("filter");
            if (filter != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = filter.DisplayName,
                    WidgetTypeId = filter.Id,
                    ComponentType = filter.ComponentType,
                    X = 4, Y = 5, W = 4, H = 5, MinW = 3, MinH = 3
                });
            }

            var dataGrid = OptimizationWidgetCatalog.FindById("data-grid");
            if (dataGrid != null)
            {
                _widgets.Add(new OptimizationWidgetInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = dataGrid.DisplayName,
                    WidgetTypeId = dataGrid.Id,
                    ComponentType = dataGrid.ComponentType,
                    X = 8, Y = 5, W = 4, H = 5, MinW = 3, MinH = 3
                });
            }
        }
    }

    #endregion

    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        return ValueTask.CompletedTask;
    }
}
