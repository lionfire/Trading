using LionFire.Trading.Alerts;
using LionFire.Trading.Notifications;
using Microsoft.Extensions.Hosting;
using System.Reactive;
using System.Reactive.Subjects;

namespace LionFire.Trading.Scanner;

// TODO: Move as much as possible of TradingAlertsDashboard Blazor component to this class?
public class ScannerVM : IHostedService
{

    #region Dependencies

    public TradingAlertsEnumerableListener TradingAlertsEnumerableListener { get; }
    public ISymbolStatsCache SymbolStatsCache { get; }
    public ILogger<ScannerVM> Logger { get; }

    #endregion

    #region Settings

    public ConcurrentDictionary<string, SymbolScannerSettings> SymbolSettings { get; set; } = new();
    public SymbolScannerSettings GetSymbolScannerSettings(string symbol) => SymbolSettings.GetOrAdd(symbol, symbol => new SymbolScannerSettings(symbol));

    #endregion

    #region Lifecycle

    public ScannerVM(TradingAlertsEnumerableListener tradingAlertsEnumerableListener, ISymbolStatsCache symbolStatsCache, ILogger<ScannerVM> logger)
    {
        TradingAlertsEnumerableListener = tradingAlertsEnumerableListener;
        SymbolStatsCache = symbolStatsCache;
        Logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Listen().FireAndForget();

        return Task.CompletedTask;
    }

    public IObservable<Unit> Changed => changed;
    private Subject<Unit> changed = new();

    CancellationTokenSource cts = new CancellationTokenSource();

    public async Task Listen()
    {
        if (cts != null) cts.Cancel();
        cts = new();

        //TradingAlertsEnumerableListener.ActiveAlerts.Connect().Subscribe(async alerts =>
        //{

        //    //var series = await Chart.AddLineSeriesAsync();
        //    //series.Data..AddPoints(alerts.Select(a => new Point(a.Time, a.Price)));

        //    changed.OnNext(Unit.Default);
        //});

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
            changed.OnNext(Unit.Default);
        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }

    #endregion

    #region Items

    Dictionary<string, ScannerSymbol> Items { get; } = new();
    private object itemsLock = new();
    public ScannerSymbol GetSymbol(string symbol)
    {
        if (Items.ContainsKey(symbol)) return Items[symbol];

        lock (itemsLock)
        {
            return Items.GetOrAdd(symbol, symbol => new ScannerSymbol(this, symbol));
        }
    }

    public ScannerSymbolTimeFrame GetSymbolTimeFrame(string symbol, TimeFrame timeFrame)
    {
        return GetSymbol(symbol).GetTimeFrame(timeFrame);
    }

    #endregion

    #region Utilities

    public double Ordering(TradingAlert scannerItem)
    {
        return (double)SymbolStatsCache.Volume24H(scannerItem.Symbol!);
    }

    #endregion

    #region OLD REVIEW

    public IEnumerable<TradingAlert> VisibleActiveAlerts
    {
        get
        {
            return
               //SymbolSettings.Where(kvp=>kvp.Value.PinnedToTop).Select(kvp=>kvp.)
               (TradingAlertsEnumerableListener.ActiveAlerts.Items
               //.Concat 
               ).OrderByDescending(a => Ordering(a))
                ;
        }
    }

    #endregion
}
