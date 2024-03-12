#nullable enable
using Orleans;

namespace LionFire.Trading;

[GenerateSerializer]
[Alias("exchange-symbol-timeframe-aspect")]
public record SymbolValueAspect(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect) : ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame)
{
    public SymbolBarsRange ToRange(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        return new SymbolBarsRange(Exchange, ExchangeArea, Symbol, TimeFrame, start, endExclusive);
    }
}


