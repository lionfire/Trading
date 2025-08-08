using LionFire.Trading.Automation.Bots;
using LionFire.Trading.HistoricalData.Retrieval;

namespace LionFire.Trading.Automation;

public abstract class PMarketProcessor<TPrecision> : IPMarketProcessor
    where TPrecision : struct, INumber<TPrecision>
{
    public int[]? InputLookbacks => [1]; // Only need current bar
    public required ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; set; }
    public HLCReference<TPrecision> Bars => new HLCReference<TPrecision>(ExchangeSymbolTimeFrame);
    IPInput IPMarketProcessor.Bars => Bars;

}

public class PAccountMarketSim<TPrecision> : PMarketProcessor<TPrecision>, IPBarsBot2
    where TPrecision : struct, INumber<TPrecision>
{

    public void Init()
    {
        // No initialization needed for this parameter class
    }
}