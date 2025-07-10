#nullable enable
using Orleans;

namespace LionFire.Trading;

[GenerateSerializer]
public record ExchangeSymbolReference(string Exchange, string ExchangeArea, string Symbol)
{

}

