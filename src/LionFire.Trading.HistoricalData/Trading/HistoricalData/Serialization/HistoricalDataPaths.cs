using LionFire.Trading.HistoricalData.Persistence;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace LionFire.Trading.HistoricalData.Serialization;

public class HistoricalDataPaths
{
    #region Config

    public string? BaseDir { get; set; }

    #endregion

    #region (Public) Methods

    public string GetDataDir(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        if (BaseDir == null) throw new ArgumentNullException(nameof(BaseDir));
        return Path.Combine(BaseDir, exchange, exchangeArea, timeFrame.Name, symbol);
    }
    public string GetDataDir(BarsRangeReference b) => GetDataDir(b.Exchange, b.ExchangeArea, b.Symbol, b.TimeFrame);

    public void CreateIfMissing()
    {
        try
        {
            if (BaseDir != null && !Directory.Exists(BaseDir)) { Directory.CreateDirectory(BaseDir); }
        }
        catch { }
    }

    public string? GetExistingPath(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, DateTime start, DateTime endExclusive, string? suffixCode = null)
    {
        var dir = GetDataDir(exchange, exchangeArea, symbol, timeFrame);

        var filename = GetFileName(start, endExclusive, suffixCode);

        string? fileResult = null;
        foreach (var file in Directory.GetFiles(dir, filename + "*"))
        {
            if (fileResult != null)
            {
                throw new Exception("Expected one match but got at least two: " + fileResult + ", " + file);
            }
            fileResult = file;
        }

        return fileResult;
    }

    #endregion

    #region (static) Methods

    public static string GetFileName(DateTime start, DateTime endExclusive, string? suffixCode = null)
    {
        var sb = new StringBuilder();

        if (start.Day == 1 && endExclusive.Day == 1 &&
            ((endExclusive.Month == start.Month + 1 && start.Year == endExclusive.Year)
            || (endExclusive.Month == 12 && start.Month == 1 && start.Year + 1 == endExclusive.Year))
        )
        {
            sb.Append(start.ToString("yyyy.MM"));
        }
        else if (
            start.Day == 1 && endExclusive.Day == 1
            && start.Month == 1 && endExclusive.Month == 1
            && endExclusive.Year == start.Year + 1)
        {
            sb.Append(start.ToString("yyyy"));
        }
        else
        {
            AppendFileNameTimeString(start, sb);
            sb.Append(" - ");
            AppendFileNameTimeString(endExclusive, sb);
        }

        if (suffixCode != null) sb.Append(suffixCode);

        return sb.ToString();
    }

    #endregion

    #region (public) File Paths

    public static void AppendFileNameTimeString(DateTime date, StringBuilder sb)
    {
        sb.Append(date.ToString("yyyy.MM.dd"));
        if (date.Hour != 0 || date.Minute != 0)
        {
            sb.Append('-');
            sb.Append(date.ToString("HH.mm"));
        }
    }



    #endregion
}


public static class HistoricalDataPathsX
{
    public static string GetFileName(this HistoricalDataPaths hdp, KlineArrayInfo info, KlineArrayFileOptions? options = null)
    {
        return HistoricalDataPaths.GetFileName(info.Start, info.EndExclusive
            , KlineArrayFileConstants.DownloadingFileExtension // Always append this here and strip it out later
            );
    }

    public static string GetPath(this HistoricalDataPaths hdp, string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, KlineArrayInfo info, KlineArrayFileOptions? options = null)
        => Path.Combine(hdp.GetDataDir(exchange, exchangeArea, symbol, timeFrame), hdp.GetFileName(info, options)) + options?.FileExtension;

}
