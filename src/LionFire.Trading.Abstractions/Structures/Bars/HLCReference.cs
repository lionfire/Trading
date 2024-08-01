#nullable enable

namespace LionFire.Trading;

public record HLCReference<TValue> : ExchangeSymbolTimeFrame, IPKlineInput, IReferenceTo<HLC<TValue>>
{
    public HLCReference(ExchangeSymbolTimeFrame e) : base(e.Exchange, e.ExchangeArea, e.Symbol, e.TimeFrame) { }
    public HLCReference(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame) : base(Exchange, ExchangeArea, Symbol, TimeFrame) { }

    public override Type ValueType => typeof(HLC<TValue>);
    public override string Key => base.Key + SymbolValueAspect.AspectSeparator + "HLC";
}
