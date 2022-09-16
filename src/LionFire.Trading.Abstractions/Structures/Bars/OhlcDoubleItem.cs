using ZeroFormatter;

namespace LionFire.Trading.HistoricalData;

[ZeroFormattable]
public struct OhlcDoubleItem
{
    [Index(0)]
    public char Code;

    [Index(1)]
    public double Open;
    [Index(2)]
    public double High;
    [Index(3)]
    public double Low;
    [Index(4)]
    public double Close;

    public OhlcDoubleItem(double open, double high, double low, double close)
    {
        Code = BarStatusCodes.Ok;
        Open = open;
        High = high;
        Low = low;
        Close = close;
    }
    public OhlcDoubleItem(char code, double open, double high, double low, double close)
    {
        Code = code;
        Open = open;
        High = high;
        Low = low;
        Close = close;
    }
}

