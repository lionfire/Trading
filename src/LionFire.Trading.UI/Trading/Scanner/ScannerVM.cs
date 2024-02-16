using LionFire.Trading.Alerts;
using System.Collections.Concurrent;

namespace LionFire.Trading.Scanner;

// TODO: Move as much as possible of TradingAlertsDashboard Blazor component to this class?
public class ScannerVM
{

    public ConcurrentDictionary<string, SymbolScannerSettings> SymbolSettings { get; set; } = new();
    public SymbolScannerSettings GetSymbolScannerSettings(string symbol) => SymbolSettings.GetOrAdd(symbol, symbol => new SymbolScannerSettings(symbol));

    public double Ordering(TradingAlert alert)
    {
        return (double)SymbolStatsCache.Volume24H(alert.Symbol);
    }

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
