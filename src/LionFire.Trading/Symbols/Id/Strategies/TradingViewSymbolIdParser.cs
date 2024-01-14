namespace LionFire.Trading.Symbols;

public class LionFireSymbolIdParser : ISymbolIdParserStrategy, IPrioritizedStragegy
{
    public float Priority => 10.0f;

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

        if (result != null && result.ExchangeCode != null)
        {
            var exchangeAreaSplit = result.ExchangeCode.Split('.', 2);
            if (exchangeAreaSplit.Length == 1)
            {
                return null;
            }
            else
            {
                result.ExchangeCode = exchangeAreaSplit[0];
                result.ExchangeAreaCode = exchangeAreaSplit[1];
            }
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
            result.ExchangeAreaCode = "futures";
        }

        if (result.SymbolCode.Contains("_"))
        {
            result.ExchangeAreaCode = "futures"; // REVIEW
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
