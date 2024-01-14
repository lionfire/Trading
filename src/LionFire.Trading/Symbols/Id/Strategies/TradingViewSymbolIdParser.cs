namespace LionFire.Trading.Symbols;

public class TradingViewSymbolIdParser : ISymbolIdParserStrategy
{
    public SymbolIdParseResult? TryParse(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return null;

        var colonSplit = symbol.Split(':');

        SymbolIdParseResult? result = null;
        SymbolIdParseResult R() => result ??= new SymbolIdParseResult { ParserName = this.GetType().Name };
        if (colonSplit.Length > 1)
        {
            var r = R();
            r.ExchangeCode = colonSplit[0];
            r.SymbolCode = colonSplit[1];
        }
        else
        {
            R().SymbolCode = symbol;
        }

        if (result!.SymbolCode!.EndsWith(".P"))
        {
            result.SymbolCode = result.SymbolCode[..^(".P".Length)];
            result.IsPerpetual = true;
        }

        if (result.SymbolCode.Contains("_"))
        {
            var underscoreSplit = result.SymbolCode.Split('_');
            if (underscoreSplit.Length > 1)
            {
                if (int.TryParse(underscoreSplit[1], out int code) && code.ToString() == underscoreSplit[1])
                {
                    result.SymbolCode = underscoreSplit[0];
                    result.FuturesExpiryDateCode = underscoreSplit[1];
                }
                else
                {
                    throw new ArgumentException("Don't know how to parse after _");
                }
            }
        }

        return result;
    }
}
