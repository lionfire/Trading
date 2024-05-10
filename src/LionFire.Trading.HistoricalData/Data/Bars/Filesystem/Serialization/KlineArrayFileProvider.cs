using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineArrayFileProvider
{

    #region Config

    BarFilesPaths HistoricalDataPaths { get; }

    public DateChunker RangeProvider { get; }


    #endregion

    #region Construction

    public KlineArrayFileProvider(IOptionsMonitor<BarFilesPaths> hdp, IConfiguration configuration, DateChunker rangeProvider)
    {
        HistoricalDataPaths = hdp.CurrentValue;
        HistoricalDataPaths.CreateIfMissing();
        RangeProvider = rangeProvider;
        Console.WriteLine($"HistoricalDataPaths.BaseDir: {HistoricalDataPaths.BaseDir}");
    }

    #endregion

    #region KlineArrayFile

    public KlineArrayFile GetFile(ExchangeSymbolTimeFrame reference, DateTimeOffset date, KlineArrayFileOptions? options = null)
    {
        var ((start, endExclusive), isLong) = RangeProvider.RangeForDate(date, reference.TimeFrame);

        var barsRangeReference = new SymbolBarsRange(reference.Exchange,reference.ExchangeArea, reference.Symbol, reference.TimeFrame, start, endExclusive);

        KlineArrayInfo info = new()
        {
            //SymbolBarsRange = new SymbolBarsRange(reference.Exchange, reference.ExchangeArea, reference.Symbol, reference.TimeFrame, start, endExclusive),
            Exchange = reference.Exchange,
            ExchangeArea = reference.ExchangeArea,
            Symbol = reference.Symbol,
            TimeFrame = reference.TimeFrame.Name,
            Start = start.UtcDateTime,
            EndExclusive = endExclusive.UtcDateTime,
        };


        var file = new KlineArrayFile(HistoricalDataPaths.GetPath(reference, info, options), barsRangeReference);

        return file;
    }

    #endregion
}
