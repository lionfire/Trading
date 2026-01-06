using CryptoExchange.Net.Objects;
using LionFire.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineArrayFileProvider
{

    #region Config

    BarFilesPaths HistoricalDataPaths { get; }

    public DateChunker RangeProvider { get; }

    private readonly ILogger<KlineArrayFileProvider>? _logger;

    #endregion

    #region Construction

    public KlineArrayFileProvider(IOptionsMonitor<BarFilesPaths> hdp, IConfiguration configuration, DateChunker rangeProvider, ILogger<KlineArrayFileProvider>? logger = null)
    {
        HistoricalDataPaths = hdp.CurrentValue;
        HistoricalDataPaths.CreateIfMissing();
        RangeProvider = rangeProvider;
        _logger = logger;
        Console.WriteLine($"HistoricalDataPaths.BaseDir: {HistoricalDataPaths.BaseDir}");
    }

    #endregion

    #region KlineArrayFile

    /// <summary>
    /// Gets the last write time of a downloading file, if it exists.
    /// Used to detect if another process is actively downloading.
    /// </summary>
    /// <returns>The last write time in UTC, or null if no downloading file exists.</returns>
    public DateTime? GetDownloadingFileLastWriteTime(ExchangeSymbolTimeFrame reference, DateTimeOffset date, KlineArrayFileOptions? options = null)
    {
        var ((start, endExclusive), isLong) = RangeProvider.RangeForDate(date, reference.TimeFrame);
        KlineArrayInfo info = new()
        {
            Exchange = reference.Exchange,
            ExchangeArea = reference.Area,
            Symbol = reference.Symbol,
            TimeFrame = reference.TimeFrame.Name,
            Start = start.UtcDateTime,
            EndExclusive = endExclusive.UtcDateTime,
        };
        var pathBase = HistoricalDataPaths.GetDownloadingPath(reference, info, options);

        // Check for any downloading files matching this pattern
        var dir = Path.GetDirectoryName(pathBase);
        var fileName = Path.GetFileName(pathBase);
        if (dir == null || !Directory.Exists(dir)) return null;

        DateTime? mostRecentWrite = null;
        foreach (var path in Directory.GetFiles(dir, fileName + "*"))
        {
            var lastWrite = File.GetLastWriteTimeUtc(path);
            if (!mostRecentWrite.HasValue || lastWrite > mostRecentWrite.Value)
            {
                mostRecentWrite = lastWrite;
            }
        }
        return mostRecentWrite;
    }

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
        DownloadLock? fileLock = null;
        try
        {
            // Step 1: Process-local coordination (fast path for same-process concurrency)
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

            // Step 2: Cross-process coordination via file-based lock
            _logger?.LogDebug("Attempting to acquire file lock for {Path}", path);
            fileLock = await DownloadLock.TryAcquireAsync(
                path,
                _logger,
                timeout: waitForDownloadInProgressToFinish ? TimeSpan.FromSeconds(60) : TimeSpan.Zero,
                cancellationToken
            ).ConfigureAwait(false);

            if (fileLock == null)
            {
                // Another process has the lock or complete file was created while waiting
                _logger?.LogDebug("Could not acquire file lock for {Path}. Another process may be downloading.", path);
                return null;
            }

            // Double-check complete file doesn't exist after acquiring lock
            if (File.Exists(completePath))
            {
                Debug.WriteLine($"Complete file already exists (stage 3, after lock): {completePath}");
                fileLock.Dispose();
                return null;
            }

            var klineArrayFile = await KlineArrayFile.Create(path, barsRangeReference).ConfigureAwait(false);
            if (klineArrayFile != null)
            {
                // Transfer lock ownership to the KlineArrayFile - it will release on Dispose
                klineArrayFile.Lock = fileLock;
                fileLock = null; // Prevent finally block from disposing
            }
            return klineArrayFile;
        }
        catch
        {
            throw;
        }
        finally
        {
            // Release lock if we still own it (wasn't transferred to KlineArrayFile)
            fileLock?.Dispose();

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
