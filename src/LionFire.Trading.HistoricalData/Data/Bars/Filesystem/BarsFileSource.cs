//using LionFire.Resolves;
using Binance.Net.Interfaces;
using DynamicData;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Persistence;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using static LionFire.Trading.HistoricalData.ListAvailableHistoricalDataCommand;

namespace LionFire.Trading.HistoricalData.Sources;

public class BarsFileSource : IChunkedBars, IListableBarsSource
{
    public string Name => "Filesystem";
    public HistoricalDataSourceKind2 SourceType => HistoricalDataSourceKind2.Local;
    public HistoricalDataSourceKind Kind => HistoricalDataSourceKind.LocalDisk;

    public IOptionsMonitor<BarFilesPaths> OptionsMonitor { get; }
    public ILogger<BarsFileSource> Logger { get; }
    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }

    #region Lifecycle

    public BarsFileSource(IOptionsMonitor<BarFilesPaths> optionsMonitor, ILogger<BarsFileSource> logger, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider)
    {
        OptionsMonitor = optionsMonitor;
        Logger = logger;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
    }

    #endregion

    #region Paths

    //public string GetExistingPath(SymbolBarsRange barsRangeReference)
    //{
    //    var path = HistoricalDataPaths.GetExistingPath(barsRangeReference);
    //}

    #endregion

    #region Try Load Chunk

    public async Task<IBarsResult?> GetShortChunk(SymbolBarsRange range, bool fallbackToLongChunkSource = false, QueryOptions? options = null)
    {
        if (!HistoricalDataChunkRangeProvider.IsValidShortRange(range.TimeFrame, range.Start, range.EndExclusive)) throw new ArgumentException($"Invalid short range: {range}");

        var chunk = await TryLoadChunk(range);

        if (chunk == null && fallbackToLongChunkSource)
        {
            var longRange = HistoricalDataChunkRangeProvider.LongRangeForDate(range.Start, range.TimeFrame);

            if (longRange.endExclusive <= DateTime.UtcNow)
            {
                chunk = await GetLongChunk(new SymbolBarsRange(range.Exchange, range.ExchangeArea, range.Symbol, range.TimeFrame, longRange.start, longRange.endExclusive), options);

                if (chunk != null) { chunk = chunk.Trim(range.Start, range.EndExclusive); }
            }
        }

        return chunk;
    }

    public Task<IBarsResult?> GetLongChunk(SymbolBarsRange range, QueryOptions? options = null)
    {
        if (!HistoricalDataChunkRangeProvider.IsValidLongRange(range.TimeFrame, range.Start, range.EndExclusive)) throw new ArgumentException($"Invalid long range: {range}");
        return TryLoadChunk(range);
    }

    public static bool DeleteExceptionFiles = true;

    private async Task<IBarsResult?> TryLoadChunk(SymbolBarsRange range)
    {
        var path = HistoricalDataPaths.GetExistingPath(range);
        if (path == null) return null;

        IBarsResult? result = await Task.Run(async () =>
        {
            try
            {
                var (info, bars) = KlineFileDeserializer.Deserialize(path);
                if (bars == null) return null;

                return new BarsResult<IKline>
                {
                    Start = range.Start,
                    EndExclusive = range.EndExclusive,
                    TimeFrame = range.TimeFrame,
                    Values = bars,
                    //NativeType = typeof(IBinanceKline),
                };
            }
            catch (Exception ex)
            {
                await Task.Delay(1000 * 1);
                Logger.LogError(ex, "Failed to load chunk from path: {path}", path);
                if (DeleteExceptionFiles)
                {
                    try
                    {
                        File.Delete(path);
                        Logger.LogWarning("Deleted erroring file: " + path);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogError(ex2, "Failed to delete erroring file: " + path);
                    }
                }
                return null;
            }
        });

        return result;
    }

    #endregion


    /// <summary>
    /// Returns the paths of all files, in chronological order, that contain bars for the specified time range.
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="exchangeArea"></param>
    /// <param name="symbol"></param>
    /// <param name="timeFrame"></param>
    /// <param name="start"></param>
    /// <param name="endExclusive"></param>
    /// <returns></returns>
    public IEnumerable<string> GetExistingPathsForBars(SymbolBarsRange barsRangeReference)
    {
        //var dir = HistoricalDataPaths.GetDataDir(exchange, exchangeArea, symbol, timeFrame);
        foreach (var chunk in HistoricalDataChunkRangeProvider.GetBarChunks(barsRangeReference))
        {
            var path = HistoricalDataPaths.GetExistingPath(barsRangeReference);
            if (path != null) yield return path;
        }
    }

    //public async Task<BarsAvailable> ListExchanges()
    //{

    //}
    //public async Task<BarsAvailable> ListExchangeAreas(string exchange)
    //{

    //}
    //public async Task<BarsAvailable> ListSymbols(string exchange, string exchangeArea)
    //{

    //}
    //public async Task<BarsAvailable> ListSymbols()
    //{
    //}

    public async Task SaveBarsInfo(SymbolBarsRange range, BarsInfo barsInfo)
    {
        await Task.Run(async () =>
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            var dir = OptionsMonitor.CurrentValue.GetDataDir(range);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
            var path = Path.Combine(dir, BarsInfo.InfoFileName);
            var yaml = serializer.Serialize(barsInfo);
            await File.WriteAllTextAsync(path, yaml);
        });
    }

    public BarFilesPaths HistoricalDataPaths => OptionsMonitor.CurrentValue;
    public string BarsInfoPath(ExchangeSymbolTimeFrame reference)
    {
        var dir = OptionsMonitor.CurrentValue.GetDataDir(reference);
        var path = Path.Combine(dir, BarsInfo.InfoFileName);
        return path;
    }

    //public Task<BarsInfo?> LoadBarsInfo(SymbolBarsRange range)
    //=> LoadBarsInfo(range.Exchange, range.ExchangeArea, range.Symbol, range.TimeFrame);
    public async Task<BarsInfo?> LoadBarsInfo(ExchangeSymbolTimeFrame reference)
    {
        var dir = OptionsMonitor.CurrentValue.GetDataDir(reference);

        BarsInfo? barsInfo = null;

        await Task.Run(async () =>
        {
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var path = Path.Combine(dir, BarsInfo.InfoFileName);
            if (!File.Exists(path)) { return; }
            var yaml = await File.ReadAllTextAsync(path);
            barsInfo = deserializer.Deserialize<BarsInfo>(yaml);
        });
        return barsInfo;
    }

    #region List

    //public Task<BarChunksAvailable> List(SymbolBarsRange range)
    //=> List(range.Exchange, range.ExchangeArea, range.Symbol, range.TimeFrame);
    public async Task<BarChunksAvailable> List(ExchangeSymbolTimeFrame r)
    {
        var result = new BarChunksAvailable();

        var hdp = OptionsMonitor.CurrentValue;

        var dir = hdp.GetDataDir(r);

        if (!Directory.Exists(dir)) { return result; }

        await Task.Run(async () =>
        {
            result.BarsInfo = await LoadBarsInfo(r);
            foreach (var path in Directory.GetFiles(dir).Where(p => !p.Contains(".downloading")))
            {
                var extension = Path.GetExtension(path);
                var filename = Path.GetFileName(path);
                if (filename == BarsInfo.InfoFileName) continue;

                KlineArrayInfo info;
                try
                {
                    info = KlineFileDeserializer.DeserializeInfo(path);
                }
                catch (Exception)
                {
                    Logger.LogWarning($"Failed to deserialize bars from file: {Path.GetFileName(path)}");
                    continue;
                }
                if (info == null)
                {
                    Logger.LogWarning("Deserialize returned null.  Deleting file: {path}", path);
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Deserialize returned null but failed to delete file: {path}", path);
                    }
                    continue;
                }

                var item = new BarsChunkInfo()
                {
                    ChunkName = Path.GetFileNameWithoutExtension(path).Replace(KlineArrayFileConstants.PartialFileExtension, ""),
                    Bars = info.Bars,
                    ExpectedBars = r.TimeFrame.GetExpectedBarCountForNow(info.Start, info.EndExclusive),
                    Start = info.Start,
                    EndExclusive = info.EndExclusive,
                };
                item.Percent = item.ExpectedBars.HasValue ? (info.Bars / (double)item.ExpectedBars).ToString("P1") : 0;
                result.Chunks.Add(item);
            }
        });
        return result;
    }

    #endregion

    public IHistoricalTimeSeries<IKline> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
    {
        throw new NotImplementedException();
    }

    public IHistoricalTimeSeries<decimal> GetSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, DataPointAspect aspect)
    {
        throw new NotImplementedException();
    }

}
