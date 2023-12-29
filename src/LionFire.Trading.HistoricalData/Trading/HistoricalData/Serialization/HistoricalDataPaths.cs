using LionFire.Trading.HistoricalData.Persistence;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace LionFire.Trading.HistoricalData.Serialization;

public class HistoricalDataPaths
{
    #region Config

    public string? BaseDir { get; set; }

    #endregion
    
    public string GetDataDir(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame)
    {
        if (BaseDir == null) throw new ArgumentNullException(nameof(BaseDir));
        return Path.Combine(BaseDir, exchange, exchangeArea, timeFrame.Name, symbol);
    }

    public string GetFileName(KlineArrayInfo info, KlineArrayFileOptions? options = null)
    {
        var sb = new StringBuilder();

        if (info.Start.Day == 1 && info.EndExclusive.Day == 1 && info.EndExclusive.Month== info.Start.Month + 1 && info.Start.Year == info.EndExclusive.Year)
        {
            sb.Append(info.Start.ToString("yyyy.MM"));
        }
        else if (
            info.Start.Day == 1 && info.EndExclusive.Day == 1
            && info.Start.Month == 1 && info.EndExclusive.Month == 1
            && info.EndExclusive.Year == info.Start.Year + 1)
        {
            sb.Append(info.Start.ToString("yyyy"));
        }
        else
        {
            AppendFileNameTimeString(info.Start, sb);
            sb.Append(" - ");
            AppendFileNameTimeString(info.EndExclusive, sb);
        }

        sb.Append(KlineArrayFileConstants.DownloadingFileExtension); // Always append this here and strip it out later

        return sb.ToString();
    }

    public string GetPath(string exchange, string exchangeArea, string symbol, TimeFrame timeFrame, KlineArrayInfo info, KlineArrayFileOptions? options = null)
        => Path.Combine(GetDataDir(exchange, exchangeArea, symbol, timeFrame), GetFileName(info, options)) + options?.FileExtension;

    public void CreateIfMissing()
    {
        try
        {
            if (BaseDir != null && !Directory.Exists(BaseDir)) { Directory.CreateDirectory(BaseDir); }
        }
        catch { }
    }

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
