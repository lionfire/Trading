using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using LionFire.Trading.HistoricalData.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog.LayoutRenderers.Wrappers;
using LionFire.Trading.HistoricalData;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Binance_;


public class HistoricalBarsChunkG : Grain, IHistoricalBarsChunkG
{
    #region Dependencies

    public ILogger<HistoricalBarsChunkG> Logger { get; }
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
    public string ExchangeArea => BarsRange.Area;
    public string Symbol => BarsRange.Symbol;
    public TimeFrame TimeFrame => BarsRange.TimeFrame;

    #endregion

    #region Lifecycle

    public HistoricalBarsChunkG(
        ILogger<HistoricalBarsChunkG> logger,
        DateChunker historicalDataChunkRangeProvider,
        ISymbolIdParser symbolIdParser,
        IChunkedBars bars
        )
    {
        Logger = logger;
        BarsService = bars;

        BarsRange = SymbolBarsRange.Parse(this.GetPrimaryKeyString(), symbolIdParser);

        if (!ValidExchangeKeys.Contains(BarsRange.Exchange.ToLowerInvariant())) throw new ArgumentException($"Exchange must be one of: {string.Join(", ", ValidExchangeKeys)}");
        if (!ValidExchangeAreaKeys.Contains(BarsRange.Area)) throw new ArgumentException($"Exchange area must be one of: {string.Join(", ", ValidExchangeAreaKeys)}");
        if (!historicalDataChunkRangeProvider.IsValidShortRange(TimeFrame, BarsRange.Start, BarsRange.EndExclusive)) throw new ArgumentException("Not a valid short chunk size.");
    }

    #endregion

    #region Implementation

    #region State (cache)

    private IReadOnlyList<IKline>? bars;

    #endregion

    public async Task<IReadOnlyList<IKline>> Bars()
    {
        bool isUpToDate = bars != null && bars.Any();

        if (isUpToDate)
        {
            if (BarsRange.EndExclusive > DateTimeOffset.UtcNow)
            {
                if (bars!.Last().CloseTime < DateTimeOffset.UtcNow - TimeFrame.TimeSpan)
                {
                    Logger.LogInformation("{name} Last close of {lastClose} is old.  Retrieving latest.", this.GetPrimaryKeyString(), bars.Last().CloseTime); // LOGCLEANUP - make this trace, maybe
                    isUpToDate = false;
                }
            }
            else if (BarsRange.ExpectedBarCount != bars!.Count)
            {
                Logger.LogInformation("{name} This chunk ended as of {endExclusive} and should be complete with {expectedBarCount}, but only {barCount} bars are present. Retrieving latest.", this.GetPrimaryKeyString(), BarsRange.EndExclusive, BarsRange.ExpectedBarCount, bars.Count); // LOGCLEANUP - make this trace, maybe
                isUpToDate = false;
            }
        }

        if (!isUpToDate)
        {
            var result = await BarsService.GetShortChunk(BarsRange);

            if (result == null) throw new Exception("Failed to retrieve bars");

            var expectedLastOpenTime = BarsRange.EndExclusive > DateTimeOffset.UtcNow ? TimeFrame.GetExpectedBarOpenTimeForLastClosedBar() : TimeFrame.AddBars(BarsRange.EndExclusive, -1);

            if (expectedLastOpenTime > DateTimeOffset.UtcNow)
            {
//#if DEBUG
                Logger.LogError("Sanity check JG28F9WJ3218ROSJ"); // REVIEW - should always be true
//#endif
            }
            else if (result.Bars == null || !result.Bars.Any())
            {
                Logger.LogInformation("{name} TODO - got no bars. Figure out if this is ok.", this.GetPrimaryKeyString());
            }
            else
            {
                if (expectedLastOpenTime > result.Bars.Last().OpenTime)
                {
                    var diff = Math.Abs(TimeFrame.GetExpectedBarCount(expectedLastOpenTime, result.Bars.Last().OpenTime)!.Value);
                    if (diff > 1)
                    {
                        Logger.LogWarning("{name} Expected last open time of {expected} but got {actual}.  Diff: {diff}", this.GetPrimaryKeyString(), expectedLastOpenTime, result.Bars.Last().OpenTime, diff);
                    }
                    else
                    {
                        Logger.LogInformation("{name} Expected last open time of {expected} but got {actual}. Diff: {diff}", this.GetPrimaryKeyString(), expectedLastOpenTime, result.Bars.Last().OpenTime, diff);
                    }
                }
            }

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
