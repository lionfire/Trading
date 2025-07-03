using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;

public class MarketSim<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    public ExchangeSymbol ExchangeSymbol { get; }

    public MarketSim(ExchangeSymbol exchangeSymbol)
    {
        ExchangeSymbol = exchangeSymbol;
    }

    // RENAME HLC
    [Signal(-1000)]
    public IReadOnlyValuesWindow<HLC<TPrecision>> Bars { get; set; } = null!;

}
