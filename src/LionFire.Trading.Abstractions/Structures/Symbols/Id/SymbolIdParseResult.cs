#nullable enable

namespace LionFire.Trading;

public class SymbolIdParseResult 
{
    public string? ParserName { get; set; }

    public string? ExchangeCode { get; set; }
    public string? SymbolCode { get; set; }
    public string? TimeFrameCode { get; set; }

    /// <summary>
    /// For non-perpetual futures
    /// </summary>
    public string? FuturesExpiryDateCode { get; set; }
    public bool? IsPerpetual { get; set; }
    public bool? IsNonPerpetualFutures { get; set; }
    public bool? IsFutures => IsPerpetual == true || IsNonPerpetualFutures == true ? true : (IsPerpetual.HasValue && IsNonPerpetualFutures.HasValue ? false : null);

}
