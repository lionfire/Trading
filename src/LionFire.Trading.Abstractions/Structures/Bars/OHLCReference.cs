#nullable enable
namespace LionFire.Trading;

public record OHLCReference<TValue> : ExchangeSymbolTimeFrame, IPKlineInput, IReferenceTo<OHLC<TValue>>
{
    public OHLCReference(ExchangeSymbolTimeFrame e) : base(e.Exchange, e.Area, e.Symbol, e.TimeFrame) { }
    public OHLCReference(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame) : base(Exchange, ExchangeArea, Symbol, TimeFrame) { }

    public override Type ValueType => typeof(OHLC<TValue>);
    public override string Key => base.Key + SymbolValueAspect.AspectSeparator + "OHLC";
}
