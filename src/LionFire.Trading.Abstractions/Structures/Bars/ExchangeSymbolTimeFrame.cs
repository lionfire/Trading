#nullable enable
using Orleans;
using System.Text;

namespace LionFire.Trading;

[GenerateSerializer]
[Alias("exchange-symbol-timeframe")]
public record  ExchangeSymbolTimeFrame(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame) : ExchangeSymbol(Exchange, ExchangeArea, Symbol), IPKlineInput
{
    //public string ToGrainId() => $"{Exchange}}";

    public override string Key => $"{Exchange}.{ExchangeArea}:{Symbol}/{TimeFrame}";
    public virtual Type ValueType => typeof(IKline);

    public static ExchangeSymbolTimeFrame Parse(string id, ISymbolIdParser symbolIdParser)
    {
        Exception InvalidFormat() => new ArgumentException("Invalid key format.  Expected: <exchange>.<area>:<symbol> <tf>");

        var s = id.Split(' ');
        if (s.Length != 2) throw InvalidFormat();

        var result = symbolIdParser.TryParse(s[0]);
        if (result == null) throw InvalidFormat();

        var Exchange = result.ExchangeCode ?? throw new ArgumentNullException(nameof(result.ExchangeCode));
        var ExchangeArea = result.ExchangeAreaCode ?? throw new ArgumentNullException(nameof(result.ExchangeAreaCode));
        var Symbol = result.SymbolCode ?? throw new ArgumentNullException(nameof(result.SymbolCode));

        return new ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame.Parse(s[1]));
    }

    public SymbolBarsRange ToRange(DateTimeOffset start, DateTimeOffset endExclusive) => SymbolBarsRange.FromExchangeSymbolTimeFrame(this, start, endExclusive);
}

public static class ExchangeSymbolTimeFrameX
{
    public static string ToId(this ExchangeSymbolTimeFrame r)
    {
        var sb = new StringBuilder();
        sb.AppendId(r);
        return sb.ToString();
    }
    public static void AppendId(this StringBuilder sb, ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
    {
        sb.Append(exchangeSymbolTimeFrame.Exchange);
        if (exchangeSymbolTimeFrame.ExchangeArea != null)
        {
            sb.Append('.');
            sb.Append(exchangeSymbolTimeFrame.ExchangeArea);
        }
        sb.Append(':');
        sb.Append(exchangeSymbolTimeFrame.Symbol);
        sb.Append(' ');
        sb.Append(exchangeSymbolTimeFrame.TimeFrame.ToShortString());
    }

}
