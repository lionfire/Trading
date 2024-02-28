using Microsoft.AspNetCore.Components.Web;
using System.Reactive.Disposables;

namespace LionFire.Trading.Scanner;

public partial class TradingScanner : IDisposable
{
    #region Settings

    // TODO: Store for user, per id'ed widget

    public string DefaultQuoteAsset { get; set; } = "USDT";

    public double Volume24HScale { get; set; } = 1_000_000;
    public string Volume24HScaleUnit { get; set; } = "M";


    #region TimeFrameSettings

    public Dictionary<string, TimeFrameScannerSettings>? TimeFrameSettings { get; set; }

    #region Derived

    private IEnumerable<string> AlertingTimeFrames
    {
        get => TimeFrameSettingsGetter(s => s.Alerts);
        set => TimeFrameSettingsSetter(value, (s, v) => s.Alerts = v);
    }

    private IEnumerable<string> VisibleTimeFrames
    {
        get => TimeFrameSettingsGetter(s => s.Visible);
        set => TimeFrameSettingsSetter(value, (s, v) => s.Visible = v);
    }

    #region (Private) Enumerable converter utils

    public IEnumerable<string> TimeFrameSettingsGetter(Predicate<TimeFrameScannerSettings> p) => TimeFrameSettings.Where(kvp => p(kvp.Value)).Select(kvp => kvp.Key);
    public void TimeFrameSettingsSetter(IEnumerable<string> e, Action<TimeFrameScannerSettings, bool> a)
    {
        foreach (var kvp in TimeFrameSettings)
        {
            a(kvp.Value, e.Contains(kvp.Key));
        }
    }

    #endregion

    #endregion

    #endregion

    #region UI Class Logic

    // ENH: Make configurable and maybe also somehow dynamic
    string VolumeClass(decimal vol)
    {
        if (vol < 10_000_000)
        {
            return "Low5";
        }
        else if (vol < 30_000_000)
        {
            return "Low4";
        }
        else if (vol < 50_000_000)
        {
            return "Low3";
        }
        else if (vol < 100_000_000)
        {
            return "Low2";
        }
        else if (vol < 150_000_000)
        {
            return "Low1";
        }
        return "";
    }

    #endregion

    #endregion

    #region Lifecycle

    CompositeDisposable disposables = new();
    override protected async Task OnInitializedAsync()
    {
        await InitTimeFrameSettings();
        await VM.StartAsync(default);
        disposables.Add(VM.Changed.Subscribe(_ => InvokeAsync(StateHasChanged).FireAndForget()));

        //await Chart.InitializationCompleted;
        await base.OnInitializedAsync();
    }
    private ValueTask InitTimeFrameSettings()
    {
        // TODO: Get settings for user
        // ENH: Get timeframes available in system

        TimeFrameSettings = new() {
            new()
            {
                TimeFrame = TimeFrame.m1,
                ChartBarsToShow = 60,
                Alerts = true,
                //CanAlert = true,
                Available = true,
                Favorite = true
            },
            new()
            {
                TimeFrame = TimeFrame.m5,
                ChartBarsToShow = 60,
                //Alerts = true,
                //CanAlert = true,
                Available = true,
                //Favorite = true
            },
            new()
            {
                TimeFrame = TimeFrame.m15,
                ChartBarsToShow = 96,
                Alerts = true,
                //CanAlert = true,
                Available = true,
                Favorite = true
            },
            new()
            {
                TimeFrame = TimeFrame.h1,
                ChartBarsToShow = 72,
                //Alerts = true,
                //CanAlert = true,
                //Available = true,
               //Favorite = true
            },
            new()
            {
                TimeFrame = TimeFrame.d3,
                ChartBarsToShow = 90,
                Alerts = true,
                //CanAlert = true,
                Available = true,
                Favorite = true
            }
        };

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        disposables?.Dispose();
    }

    #endregion

    //ChartComponent Chart { get; set; }
}