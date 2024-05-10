#nullable enable
using Orleans;

namespace LionFire.Trading;

[GenerateSerializer]
[Alias("exchange-symbol-timeframe-aspect")]
public record SymbolValueAspect(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect) : ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame)
{
    //public SymbolBarsRange ToRange(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    return new SymbolBarsRange(Exchange, ExchangeArea, Symbol, TimeFrame, start, endExclusive);
    //    //return SymbolBarsRange.FromExchangeSymbolTimeFrame(this, start, endExclusive); // base
    //}
}

public interface IValueType
{
    Type ValueType { get; }
}

public record SymbolValueAspect<TValue>(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect) : SymbolValueAspect(Exchange, ExchangeArea, Symbol, TimeFrame, Aspect), IValueType
{
    public Type ValueType => typeof(TValue);
}


