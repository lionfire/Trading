using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.HistoricalData.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog.LayoutRenderers.Wrappers;

namespace LionFire.Trading.Binance_;

public class HistoricalBarsChunkG : Grain, IHistoricalBarsChunk
{
    #region Dependencies

    //public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }
    //public BarsFileSource BarsFileSource { get; }
    //public KlineArrayFileProvider KlineArrayFileProvider { get; }
    //public BarsService BarsService { get; }
    public IChunkedBars ChunkedBars { get; }
    //HistoricalDataPaths HistoricalDataPaths { get; }

    #endregion

    #region Parameters

    #region Validation

    public HashSet<string> ValidExchangeKeys = new HashSet<string>
    {
        "BINANCE"
    };

    #endregion

    public SymbolBarsRange BarsRange { get; }
    public string Exchange => BarsRange.Exchange;
    public string ExchangeArea => BarsRange.ExchangeArea;
    public string Symbol => BarsRange.Symbol;
    public TimeFrame TimeFrame => BarsRange.TimeFrame;

    #region Derived

    //private string Dir => this.HistoricalDataPaths.GetDataDir(BarsRange);

    #endregion

    #endregion

    #region Lifecycle

    public HistoricalBarsChunkG(
        //IOptionsMonitor<HistoricalDataPaths> historicalDataPathsOptions,
        HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider,
        //BarsFileSource barsFileSource,
        //KlineArrayFileProvider klineArrayFileProvider,
        ISymbolIdParser symbolIdParser,
        //BarsService barsService, // TODO: Use this service and eliminate the rest
        IChunkedBars chunkedBars
        )
    {
        //HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
        //BarsFileSource = barsFileSource;
        //KlineArrayFileProvider = klineArrayFileProvider;
        //BarsService = barsService;
        ChunkedBars = chunkedBars;
        //HistoricalDataPaths = historicalDataPathsOptions.CurrentValue;

        BarsRange = SymbolBarsRange.Parse(this.GetPrimaryKeyString(), symbolIdParser);

        if (!ValidExchangeKeys.Contains(BarsRange.Exchange)) throw new ArgumentException($"Exchange must be one of: {string.Join(", ", ValidExchangeKeys)}");
        if (!historicalDataChunkRangeProvider.IsValidShortRange(TimeFrame, BarsRange.Start, BarsRange.EndExclusive)) throw new ArgumentException("Not a valid short chunk size.");
    }

    //public override Task OnActivateAsync(CancellationToken cancellationToken)
    //{
    //    return base.OnActivateAsync(cancellationToken);
    //}
    //public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    //{
    //    return base.OnDeactivateAsync(reason, cancellationToken);
    //}

    #endregion

    #region Implementation - TEMP: Binance specific

    private List<IBinanceKline>? bars;

    public async Task<List<IBinanceKline>> Bars()
    {
        if (bars == null)
        {
            var result = await ChunkedBars.GetShortChunk(BarsRange);

            if (result == null) throw new Exception("Failed to retrieve bars");

            bars = result.Bars.Cast<IBinanceKline>().ToList(); // TODO TEMP 
        }
        return bars;
    }

    public async Task<IEnumerable<IBinanceKline>> BarsInRange(DateTime start, DateTime endExclusive)
    {
        if (bars == null) await Bars();

        List<IBinanceKline> list = new();

        foreach (var bar in bars)
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
