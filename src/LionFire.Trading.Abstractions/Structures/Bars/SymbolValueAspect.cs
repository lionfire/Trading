#nullable enable
using Orleans;

namespace LionFire.Trading;

[GenerateSerializer]
[Alias("exchange-symbol-timeframe-aspect")]
public abstract record SymbolValueAspect(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect) : ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame), IKeyed<string>, IPInput
{
    //public SymbolBarsRange ToRange(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    return new SymbolBarsRange(Exchange, ExchangeArea, Symbol, TimeFrame, start, endExclusive);
    //    //return SymbolBarsRange.FromExchangeSymbolTimeFrame(this, start, endExclusive); // base
    //}
    public virtual string Key => $"{Exchange}.{ExchangeArea}:{Symbol}/{TimeFrame}{(Aspect == DataPointAspect.Unspecified ? "" : "#")}{Aspect}";

    public const char AspectSeparator = '#';
    public abstract Type ValueType { get; }
}

public record SymbolValueAspect<TValue>(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect) 
    : SymbolValueAspect(Exchange, ExchangeArea, Symbol, TimeFrame, Aspect)
{
    public override Type ValueType => typeof(TValue);
    public override string Key => $"{Exchange}.{ExchangeArea}:{Symbol}/{TimeFrame}{(Aspect == DataPointAspect.Unspecified ? "" : "#")}{Aspect}{(typeof(TValue) == typeof(double) ? "" : $"<{typeof(TValue).Name}>")}";
}


