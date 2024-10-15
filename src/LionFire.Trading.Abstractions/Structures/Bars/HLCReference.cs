#nullable enable

using LionFire.Serialization;
using System.Text.Json.Serialization;

namespace LionFire.Trading;


public record HLCReference<TValue> : ExchangeSymbolTimeFrame, IPKlineInput, IReferenceTo<HLC<TValue>>, ISerializableAsString
{
    public HLCReference(ExchangeSymbolTimeFrame e) : base(e.Exchange, e.ExchangeArea, e.Symbol, e.TimeFrame) { }
    public HLCReference(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame) : base(Exchange, ExchangeArea, Symbol, TimeFrame) { }

    [JsonIgnore]
    public override Type ValueType => typeof(HLC<TValue>);
    public override string Key => base.Key + SymbolValueAspect.AspectSeparator + "HLC";


    public string? Serialize() => Key;
    public static object? Deserialize(string? serializedString) { throw new NotImplementedException(); }
}
