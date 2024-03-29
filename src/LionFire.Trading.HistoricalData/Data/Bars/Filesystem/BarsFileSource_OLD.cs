﻿#if OLD
//using LionFire.Resolves;
using LionFire.Trading.HistoricalData.Persistence;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.HistoricalData.Sources;

public class BarsFileSource_OLD : HistoricalDataProvider2Base, IHistoricalDataSource2, IBarFileSources, IListableBarsSource
{
    public HistoricalDataSourceKind Kind => HistoricalDataSourceKind.LocalDisk;

    public IOptionsMonitor<BarFilesPaths> OptionsMonitor { get; }
    public ILogger<BarsFileSource> Logger { get; }
    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }

    #region Lifecycle

    public BarsFileSource_OLD(IOptionsMonitor<BarFilesPaths> optionsMonitor, ILogger<BarsFileSource> logger, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider)
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

    public Task<bool> TryLoadShortChunk()
    {
    }

    [Obsolete]
    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {

        var result = GetExistingPathsForBars();

        throw new NotImplementedException();
    }

    protected override Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }

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

    public Task<BarsInfo?> LoadBarsInfo(SymbolBarsRange range)
        => LoadBarsInfo(range.Exchange, range.ExchangeArea, range.Symbol, range.TimeFrame);
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

    public Task<BarChunksAvailable> List(SymbolBarsRange range)
        => List(range.Exchange, range.ExchangeArea, range.Symbol, range.TimeFrame);
    public async Task<BarChunksAvailable> List(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        var result = new BarChunksAvailable();

        var hdp = OptionsMonitor.CurrentValue;

        var dir = hdp.GetDataDir(new(exchange, exchangeArea, symbol, timeFrame));

        if (!Directory.Exists(dir)) { return result; }

        await Task.Run(async () =>
        {
            result.BarsInfo = await LoadBarsInfo(exchange, exchangeArea, symbol, timeFrame);
            foreach (var path in Directory.GetFiles(dir))
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

                var item = new BarsChunkInfo()
                {
                    ChunkName = Path.GetFileNameWithoutExtension(path).Replace(KlineArrayFileConstants.PartialFileExtension, ""),
                    Bars = info.Bars,
                    ExpectedBars = timeFrame.GetExpectedBarCountForNow(info.Start, info.EndExclusive),
                    Start = info.Start,
                    EndExclusive = info.EndExclusive,
                };
                item.Percent = item.ExpectedBars.HasValue ? (info.Bars / (double)item.ExpectedBars).ToString("P1") : 0;
                result.Chunks.Add(item);
            }
        });
        return result;
    }
}

#endif