using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.HistoricalData.Serialization;

/// <summary>
/// Information stored in a download lock file.
/// </summary>
public class DownloadLockInfo
{
    public int ProcessId { get; set; }
    public string? MachineName { get; set; }
    public DateTime AcquiredUtc { get; set; }
    public string? DownloadingPath { get; set; }
}

/// <summary>
/// Provides cross-process file-based locking for download operations.
/// Prevents multiple processes from downloading the same data chunk simultaneously.
/// </summary>
public class DownloadLock : IDisposable
{
    #region Constants

    public const string LockFileExtension = ".lock";

    /// <summary>
    /// Maximum age of a lock before it's considered stale, even if the process appears alive.
    /// This handles cases where a process is hung but not crashed.
    /// </summary>
    public static TimeSpan MaxLockAge = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How long to wait between retries when trying to acquire a lock.
    /// </summary>
    public static TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(500);

    #endregion

    #region Properties

    public string LockFilePath { get; }
    public string DownloadingPath { get; }
    public DownloadLockInfo? LockInfo { get; private set; }
    public bool IsAcquired => LockInfo != null;

    private readonly ILogger? _logger;
    private bool _disposed;

    #endregion

    #region Construction

    private DownloadLock(string downloadingPath, ILogger? logger = null)
    {
        DownloadingPath = downloadingPath;
        LockFilePath = downloadingPath + LockFileExtension;
        _logger = logger;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Attempts to acquire a lock for the specified downloading path.
    /// </summary>
    /// <param name="downloadingPath">Path to the .downloading file</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    /// <param name="timeout">Maximum time to wait for lock acquisition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A DownloadLock if acquired, null if unable to acquire within timeout</returns>
    public static async Task<DownloadLock?> TryAcquireAsync(
        string downloadingPath,
        ILogger? logger = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var stopwatch = Stopwatch.StartNew();
        var downloadLock = new DownloadLock(downloadingPath, logger);

        while (stopwatch.Elapsed < effectiveTimeout && !cancellationToken.IsCancellationRequested)
        {
            var result = await downloadLock.TryAcquireOnceAsync(cancellationToken).ConfigureAwait(false);

            switch (result)
            {
                case LockAcquisitionResult.Acquired:
                    return downloadLock;

                case LockAcquisitionResult.AlreadyComplete:
                    // The download was completed by another process
                    return null;

                case LockAcquisitionResult.LockedByOther:
                    // Another process has the lock, wait and retry
                    logger?.LogDebug("Lock held by another process for {Path}. Waiting {Delay}ms...",
                        downloadingPath, LockRetryDelay.TotalMilliseconds);
                    await Task.Delay(LockRetryDelay, cancellationToken).ConfigureAwait(false);
                    break;

                case LockAcquisitionResult.Error:
                    // Unexpected error, wait briefly and retry
                    await Task.Delay(LockRetryDelay, cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        logger?.LogWarning("Failed to acquire lock for {Path} within {Timeout}s",
            downloadingPath, effectiveTimeout.TotalSeconds);
        return null;
    }

    /// <summary>
    /// Reads lock information without attempting to acquire.
    /// </summary>
    public static DownloadLockInfo? ReadLockInfo(string downloadingPath)
    {
        var lockPath = downloadingPath + LockFileExtension;
        if (!File.Exists(lockPath)) return null;

        try
        {
            var json = File.ReadAllText(lockPath);
            return JsonSerializer.Deserialize<DownloadLockInfo>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a lock is currently held (and valid) for the specified path.
    /// </summary>
    public static bool IsLocked(string downloadingPath, out DownloadLockInfo? lockInfo)
    {
        lockInfo = ReadLockInfo(downloadingPath);
        if (lockInfo == null) return false;

        // Check if lock is stale
        if (IsLockStale(lockInfo))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Instance Methods

    private enum LockAcquisitionResult
    {
        Acquired,
        LockedByOther,
        AlreadyComplete,
        Error
    }

    private async Task<LockAcquisitionResult> TryAcquireOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            // First check if the complete file already exists
            var completePath = KlineArrayFile.CompletePathFromDownloadingPath(DownloadingPath);
            if (File.Exists(completePath))
            {
                _logger?.LogDebug("Complete file already exists: {Path}", completePath);
                return LockAcquisitionResult.AlreadyComplete;
            }

            // Check for existing lock
            if (File.Exists(LockFilePath))
            {
                var existingLock = ReadLockInfo(DownloadingPath);

                if (existingLock != null && !IsLockStale(existingLock))
                {
                    _logger?.LogDebug("Lock held by PID {Pid} on {Machine}, acquired {Age:F1}s ago",
                        existingLock.ProcessId, existingLock.MachineName,
                        (DateTime.UtcNow - existingLock.AcquiredUtc).TotalSeconds);
                    return LockAcquisitionResult.LockedByOther;
                }

                // Lock is stale, try to delete it
                _logger?.LogInformation("Removing stale lock file: {Path} (PID: {Pid}, Age: {Age:F1}s)",
                    LockFilePath, existingLock?.ProcessId,
                    existingLock != null ? (DateTime.UtcNow - existingLock.AcquiredUtc).TotalSeconds : 0);

                try
                {
                    File.Delete(LockFilePath);
                }
                catch (IOException ex)
                {
                    _logger?.LogWarning(ex, "Failed to delete stale lock file: {Path}", LockFilePath);
                    return LockAcquisitionResult.LockedByOther;
                }
            }

            // Try to create the lock file atomically
            LockInfo = new DownloadLockInfo
            {
                ProcessId = Environment.ProcessId,
                MachineName = Environment.MachineName,
                AcquiredUtc = DateTime.UtcNow,
                DownloadingPath = DownloadingPath
            };

            var json = JsonSerializer.Serialize(LockInfo, new JsonSerializerOptions { WriteIndented = true });

            // Ensure directory exists
            var dir = Path.GetDirectoryName(LockFilePath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Use FileMode.CreateNew for atomic creation
            await using (var stream = new FileStream(LockFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(json).ConfigureAwait(false);
            }

            _logger?.LogDebug("Acquired lock: {Path}", LockFilePath);
            return LockAcquisitionResult.Acquired;
        }
        catch (IOException ex) when (ex.Message.Contains("already exists") || ex.HResult == -2147024816) // File exists
        {
            // Another process created the lock file between our check and create
            _logger?.LogDebug("Lock file created by another process: {Path}", LockFilePath);
            return LockAcquisitionResult.LockedByOther;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error acquiring lock: {Path}", LockFilePath);
            return LockAcquisitionResult.Error;
        }
    }

    /// <summary>
    /// Releases the lock by deleting the lock file.
    /// </summary>
    public void Release()
    {
        if (LockInfo == null) return;

        try
        {
            if (File.Exists(LockFilePath))
            {
                // Verify we still own the lock before deleting
                var currentLock = ReadLockInfo(DownloadingPath);
                if (currentLock?.ProcessId == Environment.ProcessId)
                {
                    File.Delete(LockFilePath);
                    _logger?.LogDebug("Released lock: {Path}", LockFilePath);
                }
                else
                {
                    _logger?.LogWarning("Lock was taken over by another process, not releasing: {Path}", LockFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error releasing lock: {Path}", LockFilePath);
        }
        finally
        {
            LockInfo = null;
        }
    }

    #endregion

    #region Helpers

    private static bool IsLockStale(DownloadLockInfo lockInfo)
    {
        // Check age
        var age = DateTime.UtcNow - lockInfo.AcquiredUtc;
        if (age > MaxLockAge)
        {
            return true;
        }

        // Check if process is alive
        if (!IsProcessAlive(lockInfo.ProcessId, lockInfo.MachineName))
        {
            return true;
        }

        return false;
    }

    private static bool IsProcessAlive(int processId, string? machineName)
    {
        // If lock was created on a different machine, we can't check if process is alive
        // Assume it's alive and rely on MaxLockAge
        if (!string.IsNullOrEmpty(machineName) &&
            !string.Equals(machineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
        {
            return true; // Assume alive, rely on age check
        }

        try
        {
            var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            // Process doesn't exist
            return false;
        }
        catch (InvalidOperationException)
        {
            // Process has exited
            return false;
        }
        catch
        {
            // Unable to determine, assume alive
            return true;
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Release();
    }

    #endregion
}
