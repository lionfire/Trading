//using LionFire.Resolves;
using LionFire.Trading.HistoricalData.Persistence;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LionFire.Trading.HistoricalData.ListAvailableHistoricalDataCommand;

namespace LionFire.Trading.HistoricalData.Sources;

public class BarsFileSource : HistoricalDataProvider2Base, IHistoricalDataSource2, ILocalDiskHistoricalDataSource2, IListableBarsSource
{
    public HistoricalDataSourceKind Kind => HistoricalDataSourceKind.LocalDisk;

    public IOptionsMonitor<HistoricalDataPaths> OptionsMonitor { get; }
    public ILogger<BarsFileSource> Logger { get; }

    public BarsFileSource(IOptionsMonitor<HistoricalDataPaths> optionsMonitor, ILogger<BarsFileSource> logger)
    {
        OptionsMonitor = optionsMonitor;
        Logger = logger;
    }

    protected override Task<bool> CanGetImpl<T>(TimeFrame timeFrame, string symbol, DateTime Start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
    }

    protected override Task<T[]?> GetImpl<T>(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, HistoricalDataQueryParameters retrieveOptions)
    {
        throw new NotImplementedException();
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

    public async Task<BarsAvailable> List(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        var result = new BarsAvailable();
        
        var hdp = OptionsMonitor.CurrentValue;

        var dir = hdp.GetDataDir(exchange, exchangeArea, symbol, timeFrame);

        if (!Directory.Exists(dir)) { return result; }

        await Task.Run(() =>
        {

            foreach (var path in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(path);
                var filename = Path.GetFileName(path);
                KlineArrayInfo info;
                try
                {
                    info = KlineFileDeserializer.DeserializeInfo(path);
                }
                catch (Exception)
                {
                    Logger.LogTrace($"Unrecognized file: {Path.GetFileName(path)}");
                    continue;
                }

                var item = new BarsChunkInfo()
                {
                    ChunkName = Path.GetFileNameWithoutExtension(path).Replace(KlineArrayFileConstants.PartialFileExtension, ""),
                    Bars = info.Bars,
                    ExpectedBars = timeFrame.GetExpectedBarCount(info.Start, info.EndExclusive),
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

