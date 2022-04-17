using ZeroFormatter;

namespace LionFire.Trading.HistoricalData;

[ZeroFormattable]
public struct OhlcDecimalItem
{
    [Index(0)]
    public char Code;

    [Index(1)]
    public decimal Open;
    [Index(2)]
    public decimal High;
    [Index(3)]
    public decimal Low;
    [Index(4)]
    public decimal Close;

    public OhlcDecimalItem(decimal open, decimal high, decimal low, decimal close)
    {
        Code = BarStatusCodes.Ok;
        Open = open;
        High = high;
        Low = low;
        Close = close;
    }
    public OhlcDecimalItem(char code, decimal open, decimal high, decimal low, decimal close)
    {
        Code = code;
        Open = open;
        High = high;
        Low = low;
        Close = close;
    }
}

