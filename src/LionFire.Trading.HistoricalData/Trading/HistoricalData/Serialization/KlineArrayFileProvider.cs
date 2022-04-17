using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineArrayFileProvider
{

    #region Config

    HistoricalDataPaths HistoricalDataPaths { get; }

    public HistoricalDataChunkRangeProvider RangeProvider = new HistoricalDataChunkRangeProvider();

    
    #endregion

    #region Construction

    public KlineArrayFileProvider(IOptionsMonitor<HistoricalDataPaths> hdp, IConfiguration configuration)
    {
        HistoricalDataPaths = hdp.CurrentValue;
        HistoricalDataPaths.CreateIfMissing();

        Console.WriteLine($"HistoricalDataPaths.BaseDir: {HistoricalDataPaths.BaseDir}");
    }

    #endregion

    #region KlineArrayFile

    public KlineArrayFile GetFile(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, DateTime date, KlineArrayFileOptions? options = null)
    {
        var (start, exclusiveEnd) = RangeProvider.RangeForDate(date, timeFrame);

        KlineArrayInfo info = new()
        {
            Exchange = exchange,
            ExchangeArea = exchangeArea,
            Symbol = symbol,
            TimeFrame = timeFrame.Name,
            Start = start,
            EndExclusive = exclusiveEnd,
        };


        var file = new KlineArrayFile(HistoricalDataPaths.GetPath(exchange, exchangeArea, symbol, timeFrame, info, options), info);

        return file;
    }

    #endregion
}
