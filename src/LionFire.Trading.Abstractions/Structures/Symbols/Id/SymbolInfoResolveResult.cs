#nullable enable

using LionFire.FlexObjects;
using LionFire.Trading.Exchanges;

namespace LionFire.Trading;

public class SymbolInfoResolveResult : IFlex
{
    object? IFlex.FlexData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public SymbolIdParseResult? ParseResult { get; set; }
    public ExchangeInfo? Exchange { get; set; }
    public SymbolInfo? Symbol { get; set; }
    public TimeFrame? TimeFrame { get; set; }
}
