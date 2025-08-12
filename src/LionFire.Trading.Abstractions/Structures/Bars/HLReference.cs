#nullable enable

using LionFire.Serialization;
using System.Text.Json.Serialization;

namespace LionFire.Trading;

public record HLReference<TValue> : ExchangeSymbolTimeFrame, IPKlineInput, IReferenceTo<HL<TValue>>, ISerializableAsString
{
    public HLReference(ExchangeSymbolTimeFrame e) : base(e.Exchange, e.Area, e.Symbol, e.TimeFrame) { }
    public HLReference(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame) : base(Exchange, ExchangeArea, Symbol, TimeFrame) { }

    [JsonIgnore]
    public override Type ValueType => typeof(HL<TValue>);
    public override string Key => base.Key + SymbolValueAspect.AspectSeparator + "HL";

    public string? Serialize() => Key;
    public static object? Deserialize(string? serializedString) { throw new NotImplementedException(); }
}