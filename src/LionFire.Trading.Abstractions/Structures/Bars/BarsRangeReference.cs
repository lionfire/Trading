#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

public record BarsRangeReference(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DateTime Start, DateTime EndExclusive)
{
    static Exception ParseError(string input, string? msg = null) => new Exception($"Invalid key: {input}.  Must be in this format: \"BINANCE:BTCUSDT.P m1:2024.01.01-2024.01.02\" and a valid short chunk size. {msg}".TrimEnd());

    public static BarsRangeReference Parse(string text)
    {
        var s0 = text.Split(' ');
        if (s0.Length != 2) throw ParseError(text);
        var exchangeAndSymbol = s0[0].Split(':');
        if (exchangeAndSymbol.Length != 2) throw ParseError(text);

        var exchange = exchangeAndSymbol[0];
        var symbol = exchangeAndSymbol[1];
        var isPerpetual = symbol.EndsWith(".P");
        if (isPerpetual) { symbol.TrimEnd(".P"); }
        var isNonPerpetualFuture = symbol.Contains("_"); // REVIEW
        var ExchangeArea = isPerpetual || isNonPerpetualFuture ? "futures" : "spot";

        if (symbol.ToUpperInvariant() != symbol) throw ParseError(text, "Symbol must be uppercase");
        if (exchange.ToUpperInvariant() != exchange) throw ParseError(text, "Exchange must be uppercase");

        var timeFrameAndRange = s0[1].Split(':');
        if (timeFrameAndRange.Length != 2) throw ParseError(text);
        var timeFrame = TimeFrame.Parse(timeFrameAndRange[0]) ?? throw ParseError(text);

        var s2 = timeFrameAndRange[1].Split('-');
        if (s2.Length != 2) throw ParseError(text);

        var from = DateOnly.ParseExact(s2[0], "yyyy.MM.dd");
        var to = DateOnly.ParseExact(s2[1], "yyyy.MM.dd");

        return new BarsRangeReference(exchange, ExchangeArea, symbol, timeFrame, from.ToDateTime(), to.ToDateTime());
    }

    public void ThrowIfInvalid()
    {
        if (Start == default) throw new ArgumentNullException(nameof(Start));
        if (EndExclusive == default) throw new ArgumentNullException(nameof(EndExclusive));
    }
}

