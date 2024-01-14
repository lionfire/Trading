using System.Text;

namespace LionFire.Trading.HistoricalData.Orleans_;

public static class SymbolBarsRangeGrainX

{
    public static string ToGrainId(this SymbolBarsRange r)
    {
        var sb = new StringBuilder();

        sb.Append(r.Exchange);
        if (r.ExchangeArea != null)
        {
            sb.Append('.');
            sb.Append(r.ExchangeArea);
        }
        sb.Append(':');
        sb.Append(r.Symbol);

        sb.Append(' ');
        sb.Append(r.TimeFrame.ToString());
        sb.Append(':');
        sb.Append(r.Start.ToString("yyyy.MM.dd"));
        sb.Append('-');
        sb.Append(r.EndExclusive.ToString("yyyy.MM.dd"));

        return sb.ToString();
    }
}
