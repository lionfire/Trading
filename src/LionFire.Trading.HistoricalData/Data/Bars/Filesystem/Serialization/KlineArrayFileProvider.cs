using CryptoExchange.Net.Objects;
using LionFire.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="date"></param>
    /// <param name="forceDeleteNonStale"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true if the file was considered stale (or missing) and a deletion was attempted or the file doesn't exist.  False if the file exists and is not considered stale.</returns>
    public ValueTask<bool> TryDeleteStaleDownloadFile(ExchangeSymbolTimeFrame reference, DateTimeOffset date, bool forceDeleteNonStale = false)
    {
        var ((start, endExclusive), isLong) = RangeProvider.RangeForDate(date, reference.TimeFrame);

        var barsRangeReference = new SymbolBarsRange(reference.Exchange, reference.Area, reference.Symbol, reference.TimeFrame, start, endExclusive);
        KlineArrayInfo info = new()
        {
            //SymbolBarsRange = new SymbolBarsRange(reference.Exchange, reference.ExchangeArea, reference.Symbol, reference.TimeFrame, start, endExclusive),
            Exchange = reference.Exchange,
            ExchangeArea = reference.Area,
            Symbol = reference.Symbol,
            TimeFrame = reference.TimeFrame.Name,
            Start = start.UtcDateTime,
            EndExclusive = endExclusive.UtcDateTime,
        };
        var pathBase = HistoricalDataPaths.GetDownloadingPath(reference, info, null);

        bool didAnything = false;
        foreach (var path in Directory.GetFiles(Path.GetDirectoryName(pathBase), Path.GetFileName(pathBase) + "*"))
        {
            bool stale = true;

            if (!forceDeleteNonStale)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(path);
                TimeSpan timeDifference = DateTime.Now - lastWriteTime;
                if (timeDifference.TotalMinutes <= 2) stale = false; // HARDCODE
            }

            if (stale)
            {
                try
                {
                    File.Delete(path); // BLOCKING I/O
                    didAnything = true;
                }
                catch { } // EMPTYCATCH
            }
        }
        return ValueTask.FromResult(didAnything);
    }

    public async Task<KlineArrayFile?> TryCreateDownloadFile(ExchangeSymbolTimeFrame reference, DateTimeOffset date, KlineArrayFileOptions? options = null, bool waitForDownloadInProgressToFinish = true, CancellationToken cancellationToken = default)
    {
        var ((start, endExclusive), isLong) = RangeProvider.RangeForDate(date, reference.TimeFrame);

        var barsRangeReference = new SymbolBarsRange(reference.Exchange, reference.Area, reference.Symbol, reference.TimeFrame, start, endExclusive);

        KlineArrayInfo info = new()
        {
            //SymbolBarsRange = new SymbolBarsRange(reference.Exchange, reference.ExchangeArea, reference.Symbol, reference.TimeFrame, start, endExclusive),
            Exchange = reference.Exchange,
            ExchangeArea = reference.Area,
            Symbol = reference.Symbol,
            TimeFrame = reference.TimeFrame.Name,
            Start = start.UtcDateTime,
            EndExclusive = endExclusive.UtcDateTime,
        };

        var path = HistoricalDataPaths.GetDownloadingPath(reference, info, options);
        var completePath = KlineArrayFile.CompletePathFromDownloadingPath(path);

    tryAgain:
        if (File.Exists(completePath))
        {
            Debug.WriteLine($"Complete file already exists (stage 1): {completePath}");
            return null;
        }

        bool isDownloading = false;
        try
        {
            if (!DownloadInProgress.Add(path))
            {
                if (waitForDownloadInProgressToFinish)
                {
                    //if (File.Exists(path))
                    {
                        //Debug.WriteLine($"Download in progress: {path}");
                        if (cancellationToken.IsCancellationRequested) { throw new OperationCanceledException(); }

                        //if (DownloadInProgress.Contains(path))
                        {
                            Debug.WriteLine($"Download in progress, waiting for: {path}");
                            await KlineArrayFileProvider.HasNoDownloadsInProgress.WaitAsync().ConfigureAwait(false);
                            Debug.WriteLine($"Finished waiting on download: {path}");
                        }
                        //await Task.Delay(DownloadCheckDelay, cancellationToken).ConfigureAwait(false);
                        goto tryAgain;
                    }
                    // else continue to return null
                }
                return null;
            }
            else
            {
                OnDownloadStarted(path);
                isDownloading = true;
            }

            if (File.Exists(completePath))
            {
                Debug.WriteLine($"Complete file already exists (stage 2): {completePath}");
                DownloadInProgress.Remove(path);
                return null;
            }
            return await KlineArrayFile.Create(path, barsRangeReference).ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
        finally
        {
            if (isDownloading)
            {
                OnDownloadFinished(path);
                DownloadInProgress.Remove(path);
            }

        }
    }

    internal static TimeSpan DownloadCheckDelay = TimeSpan.FromSeconds(0.5);

    private static void OnDownloadStarted(string path)
    {
        HasNoDownloadsInProgress = new(false, false); // REVIEW - use own implementation of AsyncResetEvent, instead of relying on the CryptoExchange.Net one, since they may decide to change it.  Use TaskCompletionSource?
    }
    internal static void OnDownloadFinished(string path)
    {
        DownloadInProgress.Remove(path);
        HasNoDownloadsInProgress.Set();
    }
    internal static ConcurrentHashSet<string> DownloadInProgress { get; } = new();
    internal static AsyncResetEvent HasNoDownloadsInProgress { get; private set; } = new(true, false);

    #endregion
}
