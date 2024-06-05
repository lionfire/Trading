#nullable enable
using Orleans;

namespace LionFire.Trading;

//public record ExchangeArea(string Exchange, string ExchangeArea) { }
[Alias("exchange-symbol")]
[GenerateSerializer]
public record ExchangeSymbol(string Exchange, string ExchangeArea, string Symbol) : IKeyed<string>
{
    public virtual string Key => $"{Exchange}.{ExchangeArea}:{Symbol}";
}

