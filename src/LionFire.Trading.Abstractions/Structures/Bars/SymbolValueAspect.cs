#nullable enable
using Orleans;

namespace LionFire.Trading;

[GenerateSerializer]
[Alias("exchange-symbol-timeframe-aspect")]
public abstract record SymbolValueAspect(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect) : ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame), IPKlineInput
{
    //public SymbolBarsRange ToRange(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    return new SymbolBarsRange(Exchange, ExchangeArea, Symbol, TimeFrame, start, endExclusive);
    //    //return SymbolBarsRange.FromExchangeSymbolTimeFrame(this, start, endExclusive); // base
    //}
    public override string Key => $"{base.Key}{(Aspect == DataPointAspect.Unspecified ? "" : "#" + Aspect)}"; // TODO: creative output for Aspect

    public const char AspectSeparator = '#';
    public abstract Type ValueType { get; }
}

public record SymbolValueAspect<TValue>(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame, DataPointAspect Aspect)
    : SymbolValueAspect(Exchange, ExchangeArea, Symbol, TimeFrame, Aspect)
    , IReferenceTo<TValue>
    , IHasPrecision
{
    public override Type ValueType
        => Aspect switch
        {
            DataPointAspect.HLC => typeof(HLC<TValue>),
            DataPointAspect.OHLC => typeof(OHLC<TValue>),
            //DataPointAspect.Open => typeof(TValue),
            //DataPointAspect.High => typeof(TValue),
            //DataPointAspect.Low => typeof(TValue),
            _ => typeof(TValue),
        };

    public override string Key => $"{Exchange}.{Area}:{Symbol}/{TimeFrame}{(Aspect == DataPointAspect.Unspecified ? "" : "#")}{Aspect}{(typeof(TValue) == typeof(double) ? "" : $"<{typeof(TValue).Name}>")}";

    public Type PrecisionType => typeof(TValue);

    public static SymbolValueAspect<TValue> Create(ExchangeSymbolTimeFrame e, DataPointAspect aspect) => new SymbolValueAspect<TValue>(e.Exchange, e.Area, e.Symbol, e.TimeFrame, aspect);
}


