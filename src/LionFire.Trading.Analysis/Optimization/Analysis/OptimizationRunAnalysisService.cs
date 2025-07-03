using LionFire.Collections.Concurrent;
using LionFire.Persistence;
using LionFire.Persistence.Filesystem;
using LionFire.Referencing;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using LionFire.Threading;

namespace LionFire.Trading.Automation.Optimization.Analysis;

public class OptimizationRunAnalysisService : IHostedService
{
    #region Dependencies

    public OptimizationAnalysisOptions AnalysisOptions { get; }
    public BacktestOptions BacktestOptions { get; }
    public IngestOptions IngestOptions { get; }
    public ILogger<OptimizationRunAnalysisService> Logger { get; }

    #endregion

    #region Lifecycle

    public OptimizationRunAnalysisService(IOptionsMonitor<OptimizationAnalysisOptions> optimizationRunAnalysisOptions, IOptionsMonitor<BacktestOptions> backtestOptions, ILogger<OptimizationRunAnalysisService> logger, IOptionsMonitor<IngestOptions> ingestOptions)
    {
        AnalysisOptions = optimizationRunAnalysisOptions.CurrentValue;
        BacktestOptions = backtestOptions.CurrentValue;
        IngestOptions = ingestOptions.CurrentValue;
        Logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //Migrate().FireAndForget();
        //if (IngestOptions.Enabled) { await StartIngestAsync(cancellationToken); }
        return Task.CompletedTask;
    }

    protected async Task StartIngestAsync(CancellationToken cancellationToken)
    {
        watcher = new FileSystemWatcher(BacktestOptions.Dir);
        watcher.Created += Watcher_Created;
        watcher.Error += Watcher_Error;
        watcher.EnableRaisingEvents = true;

        await GetOptimizationRunsNeedingAnalysis().ConfigureAwait(false);
    }
    protected Task StopIngestAsync(CancellationToken cancellationToken)
    {
        if (watcher != null)
        {
            watcher.Error -= Watcher_Error;
            watcher.Created -= Watcher_Created;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }
        return Task.CompletedTask;
    }

    public Task ForceStart(CancellationToken cancellationToken)
    {
        return StartIngestAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (IngestOptions.Enabled) { await StopIngestAsync(cancellationToken); }
    }

    #endregion

    #region State

    private FileSystemWatcher? watcher;
    private Timer? IngestTimer;

    #endregion

    #region Event Handling

    private void Watcher_Created(object sender, FileSystemEventArgs e)
    {
        IngestTimer = new Timer(1000 * 60);
        IngestTimer.Elapsed += InjestTimer_Elapsed;
        IngestTimer.AutoReset = false;
        IngestTimer.Enabled = true;
    }

    int InjestTimerErrorCount = 0;
    int MaxInjestErrors = 10;

    private void InjestTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            Injest();
        }
        catch
        {
            MaxInjestErrors++;
            throw;
        }
        finally
        {
            if (InjestTimerErrorCount < AnalysisOptions.MaxIngestErrors)
            {
                IngestTimer.Enabled = true;
            }
        }
    }

    private void Injest()
    {
    }

    //private Task Migrate()
    //{
    //    return Task.Run(() =>
    //    {
    //        foreach (var symbolDir in Directory.GetDirectories(BacktestOptions.Dir))
    //        {
    //            var symbol = Path.GetFileName(symbolDir);
    //            foreach (var botDir in Directory.GetDirectories(symbolDir))
    //            {
    //                var bot = Path.GetFileName(botDir);
    //                foreach (var timeFrameDir in Directory.GetDirectories(botDir))
    //                {
    //                    var tf = Path.GetFileName(timeFrameDir);
    //                    foreach (var runDir in Directory.GetDirectories(timeFrameDir))
    //                    {
    //                        var startEnd = Path.GetFileName(runDir);
    //                        var split = startEnd.Split('-');
    //                        if (split.Length != 2)
    //                        {
    //                            Logger.LogWarning($"Unknown directory format: {runDir}");
    //                            continue;
    //                        }
    //                        var id = new OptimizationRunId()
    //                        {
    //                            Bot = bot,
    //                            DefaultSymbol = symbol,
    //                            DefaultTimeFrame = tf,
    //                            Start = split[0],
    //                            End = split[1],
    //                        };

    //                        var newDir = Path.Combine(IngestOptions.BacktestsRoot, OptimizationRunPath.GetRelativePath(id));
    //                        var parentDir = Path.GetDirectoryName(newDir);
    //                        if (!Directory.Exists(parentDir)) { Directory.CreateDirectory(parentDir); }
    //                        Directory.Move(runDir, newDir);
    //                    }
    //                    DeleteIfEmpty(timeFrameDir);
    //                }
    //                DeleteIfEmpty(botDir);
    //            }
    //            DeleteIfEmpty(symbolDir);
    //        }
    //    });
    //}

    private void DeleteIfEmpty(string dir)
    {
        if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
        {
            Directory.Delete(dir);
        }
    }

    private void Watcher_Error(object sender, ErrorEventArgs e)
    {
        Logger.LogError(e.GetException(), "FileWatcher error");
    }

    #endregion

    //public Task AnalyzeOptimizationRun(OptimizationRunId id)
    //{
    //    var results = new OptimizationRunAnalysisResults();

    //    var srcDir = Path.Combine(ResultsOptions.RootDir, OptimizationRunPath.GetRelativePath_Old(id));

    //}

    public string GetAnalyzedPath()
    {
        throw new NotImplementedException();
        //return AnalysisOptions.AnalyzedDir;
    }

    #region OptimizationRunsNeedingAnalysis

    private string OptimizationRunsNeedingAnalysisPath => Path.Combine(AnalysisOptions.AnalyzedDir, "OptimizationRunsNeedingAnalysis.json");
    private FileReference OptimizationRunsNeedingAnalysisFileReference => optimizationRunsNeedingAnalysisFileReference ??= new FileReference(OptimizationRunsNeedingAnalysisPath);

    private FileReference? optimizationRunsNeedingAnalysisFileReference = null;
    IReadWriteHandle<OptimizationRunsNeedingAnalysis>? optimizationRunsNeedingAnalysisFileHandle;
    private OptimizationRunsNeedingAnalysis OptimizationRunsNeedingAnalysis;

    private async Task<OptimizationRunsNeedingAnalysis> GetOptimizationRunsNeedingAnalysis(bool forceReload = false)
    {
        if (OptimizationRunsNeedingAnalysis == null || forceReload)
        {
            optimizationRunsNeedingAnalysisFileHandle ??= OptimizationRunsNeedingAnalysisFileReference.ToReadWriteHandle<OptimizationRunsNeedingAnalysis>();
            var result = await optimizationRunsNeedingAnalysisFileHandle.Get();

            OptimizationRunsNeedingAnalysis? newValue = null;
            if (result.HasValue)
            {
                newValue = result.Value;
                Logger.LogInformation($"Loaded {typeof(OptimizationRunsNeedingAnalysis).Name} at  {OptimizationRunsNeedingAnalysisFileReference} with {newValue.Ids.Count} items");
            }
            if (newValue == null)
            {
                newValue ??= new OptimizationRunsNeedingAnalysis();
                Logger.LogInformation($"Initializing {typeof(OptimizationRunsNeedingAnalysis).Name} at  {OptimizationRunsNeedingAnalysisFileReference}");
                await optimizationRunsNeedingAnalysisFileHandle.Set(newValue);
            }
            OptimizationRunsNeedingAnalysis = newValue;
        }

        return OptimizationRunsNeedingAnalysis;
    }


    #endregion


    ConcurrentHashSet<OptimizationRunId> UnanalyzedOptimizationRuns { get; } = new();

    public void MoveFileFromResultsToAnalysis(string path)
    {

    }
    private void OnUnanalyzedOptimizationRun(OptimizationRunId id)
    {
        if (UnanalyzedOptimizationRuns.Add(id))
        {

        }

    }
}
