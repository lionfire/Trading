using LionFire.Trading.HistoricalData.Persistence;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace LionFire.Trading.HistoricalData.Serialization;

/// <summary>
/// Configuration object that points to the base Directory containing Historical Data (arrays of bars).
/// 
/// Will query the disk for existing files.
/// 
/// There is some flexibility in:
/// - suffix codes (*.part.*)
/// - file extensions (*.yaml)
///
/// Out of scope:
/// - file format
/// - which type of bars are actually stored (OHLC, OHLCV, native to exchange, etc.)
/// </summary>
public class BarFilesPaths
{
    #region Config

    public string? BaseDir { get; set; }

    #endregion

    #region (Public) Methods

    public string GetDataDir(ExchangeSymbolTimeFrame r)
    {
        if (BaseDir == null) throw new ArgumentNullException(nameof(BaseDir));
        return Path.Combine(BaseDir, r.Exchange, r.ExchangeArea, r.TimeFrame.Name, r.Symbol);
    }

    public void CreateIfMissing()
    {
        try
        {
            if (BaseDir != null && !Directory.Exists(BaseDir)) { Directory.CreateDirectory(BaseDir); }
        }
        catch { }
    }

    public string? GetExistingPath(SymbolBarsRange barsRange, string? suffixCode = null)
    {
        var dir = GetDataDir(barsRange);

        var filename = GetFileName(barsRange.Start, barsRange.EndExclusive, suffixCode);

        string? fileResult = null;
        if (!Directory.Exists(dir)) return null;
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
            || (start.Month == 12 && endExclusive.Month == 1 && start.Year + 1 == endExclusive.Year))
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
    public static string GetFileName(this BarFilesPaths hdp, KlineArrayInfo info, KlineArrayFileOptions? options = null)
    {
        return BarFilesPaths.GetFileName(info.Start, info.EndExclusive
            , KlineArrayFileConstants.DownloadingFileExtension // Always append this here and strip it out later
            );
    }

    public static string GetPath(this BarFilesPaths hdp, ExchangeSymbolTimeFrame r, KlineArrayInfo info, KlineArrayFileOptions? options = null)
        => Path.Combine(hdp.GetDataDir(r), hdp.GetFileName(info, options)) + options?.FileExtension;

}
