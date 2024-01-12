using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.HistoricalData.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Binance_;


public class HistoricalBarsChunkG : Grain, IHistoricalBarsChunk
{
    #region Dependencies

    public IBinanceRestClient BinanceRestClient { get; }
    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }
    public BarsFileSource BarsFileSource { get; }
    public KlineArrayFileProvider KlineArrayFileProvider { get; }
    HistoricalDataPaths HistoricalDataPaths { get; }

    #endregion

    #region Parameters

    #region Validation

    public HashSet<string> ValidExchangeKeys = new HashSet<string>
    {
        "BINANCE"
    };

    #endregion

    public BarsRangeReference BarsRange { get; }
    public string Exchange => BarsRange.Exchange;
    public string ExchangeArea => BarsRange.ExchangeArea;
    public string Symbol => BarsRange.Symbol;
    public TimeFrame TimeFrame => BarsRange.TimeFrame;

    #region Derived

    private string Dir => this.HistoricalDataPaths.GetDataDir(BarsRange);

    #endregion

    #endregion

    #region Lifecycle

    public HistoricalBarsChunkG(IBinanceRestClient binanceRestClient, IOptionsMonitor<HistoricalDataPaths> historicalDataPathsOptions, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider, BarsFileSource barsFileSource, KlineArrayFileProvider klineArrayFileProvider)
    {
        BinanceRestClient = binanceRestClient;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
        BarsFileSource = barsFileSource;
        KlineArrayFileProvider = klineArrayFileProvider;
        HistoricalDataPaths = historicalDataPathsOptions.CurrentValue;

        BarsRange = BarsRangeReference.Parse(this.GetPrimaryKeyString());

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

    #region Implementation

    private List<IBinanceKline>? Bars { get; set; }
    private async Task<List<IBinanceKline>> GetRangeBars()
    {
        var j = ActivatorUtilities.CreateInstance<RetrieveHistoricalDataJob>(ServiceProvider);
        //var dir = this.HistoricalDataPaths.GetDataDir(Exchange.ToLowerInvariant(), ExchangeArea, Symbol, TimeFrame);
        //this.HistoricalDataChunkRangeProvider.LongRangeForDate(start, TimeFrame);
        //var file = KlineArrayFileProvider.GetFile(Exchange, ExchangeArea, Symbol, TimeFrame, start);

        List<IBinanceKline> bars;

        return await j.Execute2(BarsRange);
    }

    public async Task<IEnumerable<IBinanceKline>> GetBars(DateTime start, DateTime endExclusive)
    {
        Bars ??= await GetRangeBars();

        foreach (var bar in Bars)
        {
            if (bar.OpenTime >= start && bar.OpenTime < endExclusive)
            {
                yield return bar;
            }
        }
    }

    #endregion

}
