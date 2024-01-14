﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

public record SymbolBarsRange(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DateTime Start, DateTime EndExclusive) : ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame), IRangeWithTimeFrame
{
    static Exception ParseError(string input, string? msg = null) => new Exception($"Invalid key: {input}.  Must be in this format: \"BINANCE:BTCUSDT.P m1:2024.01.01-2024.01.02\" and a valid short chunk size. {msg}".TrimEnd());

    public static SymbolBarsRange Parse(string text, ISymbolIdParser symbolIdParser)
    {
        var s0 = text.Split(' ');
        if (s0.Length != 2) throw ParseError(text);
        var r = symbolIdParser.TryParse(s0[0]);
        if (r == null) throw ParseError(text, $"{nameof(ISymbolIdParser)} failed to parse exchange and symbol");
        if (r.ExchangeCode == null) throw ParseError(text, $"{nameof(ISymbolIdParser)} failed to parse exchange");
        if (r.SymbolCode == null) throw ParseError(text, $"{nameof(ISymbolIdParser)} failed to parse symbol");


        //var exchangeAndSymbol = s0[0].Split(':');
        //if (exchangeAndSymbol.Length != 2) throw ParseError(text);

        //var exchange = exchangeAndSymbol[0];
        //var symbol = exchangeAndSymbol[1];
        var exchange = r.ExchangeCode;
        var symbol = r.SymbolCode;
        //var isPerpetual = symbol.EndsWith(".P");
        //if (isPerpetual) { symbol.TrimEnd(".P"); }
        var isPerpetual = r.IsPerpetual == true;
        //var isNonPerpetualFuture = symbol.Contains("_"); // REVIEW
        var isNonPerpetualFuture = r.IsNonPerpetualFutures == true;

        #region TODO: Exchange-specific area

        var ExchangeArea = isPerpetual || isNonPerpetualFuture ? "futures" : "spot";

        #endregion

        #region TODO: Normalize casing after resolving SymbolInfo and ExchangeInfo

        if (symbol.ToUpperInvariant() != symbol) throw ParseError(text, "Symbol must be uppercase");
        if (exchange.ToUpperInvariant() != exchange) throw ParseError(text, "Exchange must be uppercase");

        #endregion

        #region TimeFrame and Date range

        var timeFrameAndRange = s0[1].Split(':');
        if (timeFrameAndRange.Length != 2) throw ParseError(text);
        var timeFrame = TimeFrame.Parse(timeFrameAndRange[0]) ?? throw ParseError(text);

        var s2 = timeFrameAndRange[1].Split('-');
        if (s2.Length != 2) throw ParseError(text);

        var from = DateOnly.ParseExact(s2[0], "yyyy.MM.dd");
        var to = DateOnly.ParseExact(s2[1], "yyyy.MM.dd");

        #endregion

        return new SymbolBarsRange(exchange, ExchangeArea, symbol, timeFrame, from.ToDateTime(), to.ToDateTime());
    }


    public void ThrowIfInvalid()
    {
        if (Start == default) throw new ArgumentNullException(nameof(Start));
        if (EndExclusive == default) throw new ArgumentNullException(nameof(EndExclusive));
    }
}

