using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.HistoricalData.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog.LayoutRenderers.Wrappers;
using LionFire.Trading.HistoricalData;

namespace LionFire.Trading.Binance_;


public class HistoricalBarsChunkG : Grain, IHistoricalBarsChunkG
{
    #region Dependencies

    public IChunkedBars BarsService { get; }
   

    #endregion

    #region Parameters

    #region Validation

    public HashSet<string> ValidExchangeKeys = new HashSet<string>
    {
        "binance"
    };
    public HashSet<string> ValidExchangeAreaKeys = new HashSet<string>
    {
        "futures",
        "spot"
    };

    #endregion

    public SymbolBarsRange BarsRange { get; }
    public string Exchange => BarsRange.Exchange;
    public string ExchangeArea => BarsRange.ExchangeArea;
    public string Symbol => BarsRange.Symbol;
    public TimeFrame TimeFrame => BarsRange.TimeFrame;

    #endregion

    #region Lifecycle

    public HistoricalBarsChunkG(
        HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider,
        ISymbolIdParser symbolIdParser,
        IChunkedBars bars
        )
    {
        BarsService = bars;

        BarsRange = SymbolBarsRange.Parse(this.GetPrimaryKeyString(), symbolIdParser);

        if (!ValidExchangeKeys.Contains(BarsRange.Exchange.ToLowerInvariant())) throw new ArgumentException($"Exchange must be one of: {string.Join(", ", ValidExchangeKeys)}");
        if (!ValidExchangeAreaKeys.Contains(BarsRange.ExchangeArea)) throw new ArgumentException($"Exchange area must be one of: {string.Join(", ", ValidExchangeAreaKeys)}");
        if (!historicalDataChunkRangeProvider.IsValidShortRange(TimeFrame, BarsRange.Start, BarsRange.EndExclusive)) throw new ArgumentException("Not a valid short chunk size.");
    }

    #endregion

    #region Implementation

    #region State (cache)

    private IReadOnlyList<IKline>? bars;
    
    #endregion

    public async Task<IReadOnlyList<IKline>> Bars()
    {
        // TODO: Up to date check
        if (bars == null)
        {
            var result = await BarsService.GetShortChunk(BarsRange);

            if (result == null) throw new Exception("Failed to retrieve bars");

            bars = result.Bars; 
        }
        return bars;
    }

    public async Task<IEnumerable<IKline>> BarsInRange(DateTime start, DateTime endExclusive)
    {
        if (bars == null) await Bars();

        List<IKline> list = new();

        foreach (var bar in bars!)
        {
            if (bar.OpenTime >= start && bar.OpenTime < endExclusive)
            {
                list.Add(bar);
            }
        }
        return list;
    }

    #endregion

}
