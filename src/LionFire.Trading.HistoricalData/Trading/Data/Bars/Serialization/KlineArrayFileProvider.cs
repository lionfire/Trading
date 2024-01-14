using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineArrayFileProvider
{

    #region Config

    BarFilesPaths HistoricalDataPaths { get; }

    public HistoricalDataChunkRangeProvider RangeProvider { get; }


    #endregion

    #region Construction

    public KlineArrayFileProvider(IOptionsMonitor<BarFilesPaths> hdp, IConfiguration configuration, HistoricalDataChunkRangeProvider rangeProvider)
    {
        HistoricalDataPaths = hdp.CurrentValue;
        HistoricalDataPaths.CreateIfMissing();
        RangeProvider = rangeProvider;
        Console.WriteLine($"HistoricalDataPaths.BaseDir: {HistoricalDataPaths.BaseDir}");
    }

    #endregion

    #region KlineArrayFile

    public KlineArrayFile GetFile(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, DateTime date, KlineArrayFileOptions? options = null)
    {
        var (start, endExclusive) = RangeProvider.RangeForDate(date, timeFrame);

        var barsRangeReference = new SymbolBarsRange(exchange, exchangeArea, symbol, timeFrame, start, endExclusive);

        KlineArrayInfo info = new()
        {
            Exchange = exchange,
            ExchangeArea = exchangeArea,
            Symbol = symbol,
            TimeFrame = timeFrame.Name,
            Start = start,
            EndExclusive = endExclusive,
        };


        var file = new KlineArrayFile(HistoricalDataPaths.GetPath(exchange, exchangeArea, symbol, timeFrame, info, options), barsRangeReference);

        return file;
    }

    #endregion
}
