using Microsoft.AspNetCore.Components;
using ReactiveUI.Blazor;

namespace LionFire.Trading.Automation.Blazor.Optimization.Dashboard;

/// <summary>
/// Base class for optimization dashboard widgets. Provides access to the shared OneShotOptimizeVM context.
/// </summary>
public abstract class OptimizationWidgetBase : ReactiveComponentBase<OneShotOptimizeVM>
{
    /// <summary>
    /// The widget instance containing settings and layout information.
    /// </summary>
    [Parameter]
    public OptimizationWidgetInstance? WidgetInstance { get; set; }

    /// <summary>
    /// Callback when widget settings change (triggers layout save).
    /// </summary>
    [Parameter]
    public EventCallback<OptimizationWidgetInstance> OnSettingsChanged { get; set; }

    /// <summary>
    /// Gets a setting value from the widget instance.
    /// </summary>
    protected T? GetSetting<T>(string key, T? defaultValue = default)
    {
        if (WidgetInstance?.Settings == null || !WidgetInstance.Settings.TryGetValue(key, out var value))
            return defaultValue;

        if (value is T typedValue)
            return typedValue;

        // Handle JSON deserialization where numbers might come back as different types
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Sets a setting value and notifies the dashboard to save.
    /// </summary>
    protected async Task SetSettingAsync<T>(string key, T value)
    {
        if (WidgetInstance == null) return;

        WidgetInstance.Settings ??= new Dictionary<string, object>();
        WidgetInstance.Settings[key] = value!;

        if (OnSettingsChanged.HasDelegate)
        {
            await OnSettingsChanged.InvokeAsync(WidgetInstance);
        }
    }
}
