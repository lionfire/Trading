using LionFire.Trading.Alerts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Scanner;
public class ScannerSymbolTimeFrameItem

{
    public required ScannerSymbolItem Parent { get; init; }
    public string TimeFrame { get; }

    public ScannerSymbolTimeFrameItem(string timeFrame)
    {
        TimeFrame = timeFrame;
    }

    public IReadOnlyDictionary<string, TradingAlert>? Signals { get; }
    public IEnumerable<TradingAlert> SignalsList => sortedSignals?.Values ?? Enumerable.Empty<TradingAlert>();
    private SortedList<string, TradingAlert> sortedSignals = new();

    Dictionary<string, TradingAlert>? signals;
    private object signalsLock = new();
    public void SetSignal(TradingAlert alert)
    {
        signals ??= new();
        if (signals.ContainsKey(alert.Key ?? throw new ArgumentNullException()))
        {
            signals[alert.Key ?? throw new ArgumentNullException()] = alert;
        }
        else
        {
            lock (signalsLock)
            {
                signals[alert.Key ?? throw new ArgumentNullException()] = alert;
            }
        }
    }

    public IEnumerable<IKline>? Bars { get; set; }
}

