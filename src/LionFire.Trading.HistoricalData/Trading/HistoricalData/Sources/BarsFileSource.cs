//using LionFire.Resolves;
using DynamicData;
using LionFire.Trading.HistoricalData.Persistence;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.Intrinsics.Arm;
using static LionFire.Trading.HistoricalData.ListAvailableHistoricalDataCommand;

namespace LionFire.Trading.HistoricalData.Sources;

// TODO REVIEW REFACTOR - clean up all of these classes and interfaces
public class BarsFileSource : HistoricalDataProvider2Base, IHistoricalDataSource2, ILocalDiskHistoricalDataSource2, IListableBarsSource
{
    public HistoricalDataSourceKind Kind => HistoricalDataSourceKind.LocalDisk;

    public IOptionsMonitor<HistoricalDataPaths> OptionsMonitor { get; }
    public ILogger<BarsFileSource> Logger { get; }
    public HistoricalDataChunkRangeProvider HistoricalDataChunkRangeProvider { get; }

    public BarsFileSource(IOptionsMonitor<HistoricalDataPaths> optionsMonitor, ILogger<BarsFileSource> logger, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider)
    {
        OptionsMonitor = optionsMonitor;
        Logger = logger;
        HistoricalDataChunkRangeProvider = historicalDataChunkRangeProvider;
    }

    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
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
    public IEnumerable<string> GetPathsForBars(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, DateTime start, DateTime endExclusive)
    {
        //var dir = HistoricalDataPaths.GetDataDir(exchange, exchangeArea, symbol, timeFrame);
        foreach (var chunk in HistoricalDataChunkRangeProvider.GetBarChunks(start, endExclusive, timeFrame))
        {
            yield return HistoricalDataPaths.GetExistingPath(exchange, exchangeArea, symbol, timeFrame, chunk.Item1, chunk.Item2);
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



    public async Task SaveBarsInfo(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, BarsInfo barsInfo)
    {
        await Task.Run(async () =>
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            var dir = OptionsMonitor.CurrentValue.GetDataDir(exchange, exchangeArea, symbol, timeFrame);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
            var path = Path.Combine(dir, BarsInfo.InfoFileName);
            var yaml = serializer.Serialize(barsInfo);
            await File.WriteAllTextAsync(path, yaml);
        });
    }

    public HistoricalDataPaths HistoricalDataPaths => OptionsMonitor.CurrentValue;
    public string BarsInfoPath(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        var dir = OptionsMonitor.CurrentValue.GetDataDir(exchange, exchangeArea, symbol, timeFrame);
        var path = Path.Combine(dir, BarsInfo.InfoFileName);
        return path;
    }

    public async Task<BarsInfo?> LoadBarsInfo(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        var dir = OptionsMonitor.CurrentValue.GetDataDir(exchange, exchangeArea, symbol, timeFrame);

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

    public async Task<BarsAvailable> List(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        var result = new BarsAvailable();

        var hdp = OptionsMonitor.CurrentValue;

        var dir = hdp.GetDataDir(exchange, exchangeArea, symbol, timeFrame);

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

public class BarsInfo
{
    public const string InfoFileName = "info.yaml";

    public DateTime FirstOpenTime { get; set; }
}