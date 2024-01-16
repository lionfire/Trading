using System.Text;

namespace LionFire.Trading.HistoricalData.Orleans_;

public static class SymbolBarsRangeGrainX

{
    public static string ToId(this SymbolBarsRange r)
    {
        var sb = new StringBuilder();
        sb.AppendId(r);
        return sb.ToString();
    }

    public static void AppendId(this StringBuilder sb, SymbolBarsRange symbolBarsRange)
    {
        sb.AppendId((ExchangeSymbolTimeFrame)symbolBarsRange);

        //sb.Append(' ');
        //sb.Append(symbolBarsRange.TimeFrame.ToString());
        sb.Append(':');
        sb.Append(symbolBarsRange.Start.ToString("yyyy.MM.dd"));
        sb.Append('-');
        sb.Append(symbolBarsRange.EndExclusive.ToString("yyyy.MM.dd"));
    }
}
