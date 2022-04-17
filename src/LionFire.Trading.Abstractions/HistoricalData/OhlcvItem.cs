using ZeroFormatter;

namespace LionFire.Trading.HistoricalData;

[ZeroFormattable]
public struct OhlcvItem
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

    [Index(5)]
    public decimal Volume;


    public OhlcvItem(decimal open, decimal high, decimal low, decimal close, decimal volume)
    {
        Code = BarStatusCodes.Ok;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

    public OhlcvItem(char code, decimal open, decimal high, decimal low, decimal close, decimal volume)
    {
        Code = code;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }
}

