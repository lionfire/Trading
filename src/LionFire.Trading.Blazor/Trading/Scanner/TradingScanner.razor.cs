
using LionFire.Trading.Alerts;

namespace LionFire.Trading.Scanner;

public partial class TradingScanner : IDisposable
{
    public ScannerVM VM { get; set; } = new();

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

    CancellationTokenSource cts = new CancellationTokenSource();

    override protected async Task OnInitializedAsync()
    {
        await InitTimeFrameSettings();
        Listen().FireAndForget();
        //await Chart.InitializationCompleted;
        await base.OnInitializedAsync();
    }
    private ValueTask InitTimeFrameSettings()
    {
        // TODO: Get settings for user
        // ENH: Get timeframes available in system

        TimeFrameSettings =
        [
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
            },
        ];


        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        cts?.Cancel();
    }

    #endregion

    #region State

    #region Items

    Dictionary<string, ScannerSymbolItem> Items { get; } = new();
    private object itemsLock = new();
    public ScannerSymbolItem GetSymbol(string symbol)
    {
        if (Items.ContainsKey(symbol)) return Items[symbol];

        lock (itemsLock)
        {
            return Items.GetOrAdd(symbol, symbol => new ScannerSymbolItem(symbol) { Parent = this });
        }
    }

    public ScannerSymbolItem GetSymbolTimeFrame(string symbol, TimeFrame timeFrame)
    {
        return GetSymbol(symbol).GetTimeFrame(timeFrame);
    }

    #endregion

   
    #endregion

    #region Loop

    ChartComponent Chart { get; set; }

    public async Task Listen()
    {
        if (cts != null) cts.Cancel();
        cts = new();

        TradingAlertsEnumerableListener.ActiveAlerts.Connect().Subscribe(async alerts =>
        {

            //var series = await Chart.AddLineSeriesAsync();
            //series.Data..AddPoints(alerts.Select(a => new Point(a.Time, a.Price)));

            InvokeAsync(StateHasChanged).FireAndForget();
        });

        await foreach (var signal in TradingAlertsEnumerableListener.Alerts.WithCancellation(cts.Token))
        {
            if (string.IsNullOrWhiteSpace(signal.Key) || string.IsNullOrWhiteSpace(signal.Symbol) || string.IsNullOrWhiteSpace(signal.TimeFrame?.Name))
            {
                Logger.LogWarning("Ignoring alert with missing field(s): {alert}", signal);
                continue;
            }

            GetSymbol(signal.Symbol).GetTimeFrame(signal.TimeFrame).SetSignal(signal);

            //if (alert.IsTriggered)
            //{
            //    alerts.AddOrUpdate(alert.Key, alert, (k, v) => alert);
            //}
            //else
            //{
            //    alerts.TryRemove(alert.Key, out _);
            //}
            await InvokeAsync(StateHasChanged);
        }

    }

    #endregion
}