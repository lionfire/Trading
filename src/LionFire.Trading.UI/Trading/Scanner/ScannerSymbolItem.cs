namespace LionFire.Trading.Scanner;

public class ScannerSymbolItem
{
    public ScannerSymbolItem(string symbol)
    {
        Symbol = symbol;
    }

    public string Symbol { get; }

    public double Volume24H { get; set; }

    public SymbolScannerSettings Settings => Parent.GetSymbolScannerSettings(Symbol);

    public Dictionary<string, ScannerSymbolTimeFrameItem> TimeFrames { get; } = new();
    public ScannerVM Parent { get; internal set; }

    private object TimeFramesLock = new();

    public ScannerSymbolTimeFrameItem GetTimeFrame(TimeFrame timeFrame)
    {
        if (TimeFrames.ContainsKey(timeFrame.Name)) return TimeFrames[timeFrame.Name];

        lock (TimeFramesLock)
        {
            return TimeFrames.GetOrAdd(timeFrame.Name, timeFrame => new ScannerSymbolTimeFrameItem(timeFrame) { Parent = this });
        }
    }
}
