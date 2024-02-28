namespace LionFire.Trading.Scanner;

public class ScannerSymbol
{
    #region Identity

    public ScannerVM Parent { get; }
    public string Symbol { get; }

    #endregion

    #region Settings

    #region Derived
    
    public SymbolScannerSettings Settings => Parent.GetSymbolScannerSettings(Symbol);

    #endregion

    #endregion

    #region Lifecycle

    public ScannerSymbol(ScannerVM parent, string symbol)
    {
        Parent = parent;
        Symbol = symbol;
    }

    #endregion

    #region State

    public double Volume24H { get; set; }

    #endregion

    #region Children

    public Dictionary<string, ScannerSymbolTimeFrame> TimeFrames { get; } = new();
    private object TimeFramesLock = new();

    public ScannerSymbolTimeFrame GetTimeFrame(TimeFrame timeFrame)
    {
        if (TimeFrames.ContainsKey(timeFrame.Name)) return TimeFrames[timeFrame.Name];

        lock (TimeFramesLock)
        {
            return TimeFrames.GetOrAdd(timeFrame.Name, timeFrame => new ScannerSymbolTimeFrame(timeFrame) { Parent = this });
        }
    }
    #endregion


}
