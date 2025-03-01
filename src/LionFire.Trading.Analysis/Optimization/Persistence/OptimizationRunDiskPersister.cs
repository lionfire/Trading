using LionFire.Trading.Backtesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using LionFire;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Collections;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Automation.Optimization.Analysis;
using LionFire.Execution;
using LionFire.IO;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Polly;
using Polly.Retry;
using LionFire.Collections.Concurrent;

namespace LionFire.Trading.Automation.Optimization;

public static class JsonCompatibility
{
    public static string Fix(string unfixedJson)
    {
        return unfixedJson.Replace(":NaN", ":\"NaN\"");
    }
}

public class OptimizationRunDiskPersister : IOptimizationRepository
{
    #region Configuration

    public string OptimizationResultsDir => BacktestOptionsMonitor.CurrentValue.Dir;

    public IOptionsMonitor<BacktestOptions> BacktestOptionsMonitor { get; }

    JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    #endregion

    #region Dependencies

    public SymbolNameNormalizer SymbolNameNormalizer { get; }
    public IVirtualFilesystem VirtualFilesystem { get; }
    public ILogger<OptimizationRunDiskPersister> Logger { get; }

    #endregion

    #region Lifecycle

    public OptimizationRunDiskPersister(IOptionsMonitor<BacktestOptions> optimizationResultsConfig, SymbolNameNormalizer symbolNameNormalizer, IVirtualFilesystem virtualFilesystem, ILogger<OptimizationRunDiskPersister> logger)
    {
        BacktestOptionsMonitor = optimizationResultsConfig;
        SymbolNameNormalizer = symbolNameNormalizer;
        VirtualFilesystem = virtualFilesystem;
        Logger = logger;
    }

    #endregion

    #region State

    ConcurrentHashSet<OptimizationRunId> inProgress = new();

    #region Cache

    ConcurrentDictionary<OptimizationRunId, OptimizationRunBacktests> cache = new();

    public IEnumerable<string> Markets { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Bots { get; set; } = Enumerable.Empty<string>();

    #endregion

    #endregion

    #region Methods

    public void ClearCache()
    {
        cache.Clear();
        Markets = Enumerable.Empty<string>();
        Bots = Enumerable.Empty<string>();
    }

    #endregion

    #region Paths

    public string RelativePathForId(OptimizationRunId id) => Path.Combine(id.Bot, id.Symbol, id.TimeFrame, id.StartAndEnd);
    public string PathForId(OptimizationRunId id) => Path.Combine(OptimizationResultsDir, RelativePathForId(id));

    #endregion

    #region Persistence

    public async Task<OptimizationRun> Load(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default)
    {
        var result = new OptimizationRun
        {
            Id = id,
        };

        await LoadInPlace(result, progressListener, refresh, cancellationToken);

        return result;
    }

    protected Task LoadInPlace(OptimizationRun optimizationRun, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            Task.Run(async () =>
            {
                optimizationRun.Notes = await GetNotes(optimizationRun.Id, progressListener, refresh, cancellationToken);
            }, cancellationToken)
            , Task.Run(async () =>
            {
                optimizationRun.Backtests = await LoadBacktests(optimizationRun.Id, progressListener, refresh, cancellationToken);
            }, cancellationToken)
            , Task.Run(async () =>
            {
                optimizationRun.Stats = await GetStats(optimizationRun.Id, progressListener, refresh, cancellationToken);
            }, cancellationToken)
        );
    }

    #region Stats

    public async Task<OptimizationRunStats?> GetStats(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default)
    {
        var statsPath = PathForStats(id);

        OptimizationRunStats? result = null;

        if (!refresh && File.Exists(statsPath))
        {
            var backtestsCount = await LoadBacktestsCount(id, refresh, cancellationToken);

            result = JsonSerializer.Deserialize<OptimizationRunStats>(File.ReadAllText(statsPath), JsonSerializerOptions);

            if (result == null) return result;
            else if (result.BacktestsCount != backtestsCount)
            {
                Logger.LogInformation($"Detected backtests change.  Was {result.BacktestsCount} but is now {backtestsCount}.  Regenerating stats for: {id}");
            }
            else if (result.Version < OptimizationRunStats.CurrentVersion)
            {
                Logger.LogInformation($"Backtest Stats is old version ({result.Version}).  Recalculating with new version ({OptimizationRunStats.CurrentVersion})");
            }
            else
            {
                return result;
            }
        }

        var backtests = await LoadBacktests(id, progressListener, refresh, cancellationToken);
        if (backtests != null)
        {
            await Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                result = OptimizationRunStatsGenerator.Generate(backtests);
                Logger.LogDebug("Calculating stats took {0}ms", sw.ElapsedMilliseconds.ToString());
            });
            await retryPolicy.ExecuteAsync(() => File.WriteAllTextAsync(statsPath, JsonSerializer.Serialize(result, JsonSerializerOptions)));
        }

        return result;
    }
    AsyncRetryPolicy retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));



    #endregion

    #region Notes

    public string PathForStats(OptimizationRunId id) => Path.Combine(PathForId(id), "_stats.json");
    public string PathForNotes(OptimizationRunId id) => Path.Combine(PathForId(id), "_notes.json");

    public async Task<OptimizationRunNotes?> GetNotes(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default)
    {
        var path = PathForNotes(id);

        if (!await VirtualFilesystem.FileExists(path).ConfigureAwait(false)) { return null; }

        var notes = JsonSerializer.Deserialize<OptimizationRunNotes>(
            await VirtualFilesystem.ReadAllText(path, cancellationToken).ConfigureAwait(false), JsonSerializerOptions
            );

        return notes;
    }

    public async Task SetNotes(OptimizationRunId id, OptimizationRunNotes notes)
    {
        var path = PathForNotes(id);
        if (!await VirtualFilesystem.FileExists(path).ConfigureAwait(false)) { VirtualFilesystem.DeleteFile(path); }

        await Task.Run(async () =>
        {
            await VirtualFilesystem.WriteText(path, JsonSerializer.Serialize<OptimizationRunNotes>(notes, JsonSerializerOptions));
        });
    }

    #endregion

    #region Backtests

    public Task<int> LoadBacktestsCount(OptimizationRunId id, bool refresh = false, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (!refresh)
            {
                if (cache.TryGetValue(id, out var cached2))
                {
                    return cached2.Backtests.Count;
                }
            }

            var sw = Stopwatch.StartNew();
            var dir = Path.Combine(OptimizationResultsDir, OptimizationRunPath.GetRelativePath(id));

            var fileCount = Directory.GetFiles(dir).Where(p => !p.Contains(".trades.") && p.EndsWith(".json") && !Path.GetFileName(p).StartsWith("_")).Count();

            Logger.LogDebug($"{fileCount} backtests detected in {sw.ElapsedMilliseconds}ms.");

            if (cache.TryGetValue(id, out var cached) && cached.Backtests.Count != fileCount)
            {
                cache.TryRemove(id, out var _);
            }

            return fileCount;
        });
    }

    public bool AutoDeleteEmptyFiles = true;
    public bool AutoDeleteEmptyDirectories = true;

    public async Task<OptimizationRunBacktests?> LoadBacktests(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default)
    {
        bool addedToInProgress = false;
        try
        {
            if (!refresh)
            {
                #region inProgress

                while (!inProgress.Add(id))
                {
                    await Task.Delay(100);
                    if (cancellationToken.IsCancellationRequested) { return null; }
                }
                addedToInProgress = true;

                #endregion  

                if (cache.TryGetValue(id, out var cached))
                {
                    progressListener?.OnProgress?.Invoke(cached.Backtests.Count, cached.Backtests.Count);
                    return cached;
                }
            }

            var dir = Path.Combine(OptimizationResultsDir, OptimizationRunPath.GetRelativePath(id));

            OptimizationRunBacktests optimizationRun = new();

            var files = Directory.GetFiles(dir)
                .Where(p =>
                !p.Contains(".trades.")
                && p.EndsWith(".json")
                && !Path.GetFileName(p).StartsWith("_"));
            var fileCount = files.Count();

            if (fileCount == 0 && AutoDeleteEmptyDirectories)
            {
                if (Directory.GetFiles(dir).Length == 0)
                {
                    Logger.LogWarning($"Deleting empty directory: {dir}");
                    Directory.Delete(dir);
                }
            }
            var sw = Stopwatch.StartNew();

            progressListener?.OnProgress?.Invoke(optimizationRun.Backtests.Count, fileCount);
            foreach (var path in files)
            {
                var json = await File.ReadAllTextAsync(path);
                var length = json?.Trim().Length;
                if (!length.HasValue || length.Value == 0)
                {
                    if (AutoDeleteEmptyFiles)
                    {
                        Logger.LogWarning($"Deleting empty file: {path}");
                        File.Delete(path);
                    }
                    else
                    {
                        Logger.LogWarning($"Ignoring empty file: {path}");
                    }
                    continue;
                }
                BacktestResult? backtestResult;
                try
                {
                    backtestResult = JsonSerializer.Deserialize<BacktestResult>(json, JsonSerializerOptions)!;
                }
                catch (JsonException ex)
                {
                    if (ex.Message.Contains("'N' is an invalid start of a value."))
                    {
                        json = JsonCompatibility.Fix(await File.ReadAllTextAsync(path, cancellationToken));
                        //fs.Dispose();
                        File.Move(path, path + ".broken");
                        File.WriteAllText(path, json);
                        //fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        backtestResult = JsonSerializer.Deserialize<BacktestResult>(json, JsonSerializerOptions);
                    }
                    else
                    {
                        throw;
                    }
                }
                if (backtestResult == null)
                {
                    Logger.LogWarning($"deserialized null: {path}");
                    continue;
                }

                if (backtestResult.AD == 0) { backtestResult.AD = 100.0 * backtestResult.Aroi / backtestResult.MaxEquityDrawdownPercentages; }


                optimizationRun.Backtests.Add(backtestResult);
                progressListener?.OnProgress?.Invoke(optimizationRun.Backtests.Count, fileCount);

                //fs.Seek(0, SeekOrigin.Begin);
                PopulateParametersFromStream(json, optimizationRun.Parameters);
                if (fileCount > 5000 && optimizationRun.Backtests.Count % 100 == 0)
                {
                    Console.Write(".");
                }
                if (cancellationToken.IsCancellationRequested) { return null; }
            }

            Logger.LogDebug($"{fileCount} backtests loaded in {sw.ElapsedMilliseconds}ms.  ( {sw.ElapsedMilliseconds / (double)fileCount}ms / backtest)");

            cache.AddOrUpdate(id, optimizationRun, (i, e) => optimizationRun);
            return optimizationRun;
        }
        finally
        {
            if (addedToInProgress)
            {
                inProgress.Remove(id);
            }
        }
    }

    #endregion

    #region Runs

    public Task<IEnumerable<OptimizationRun>> GetRuns(string? botName = null)
    {
        return Task.Run<IEnumerable<OptimizationRun>>(() =>
        {
            var runs = new List<OptimizationRun>();

            var symbols = new HashSet<string>();
            var bots = new HashSet<string>();

            var botDirs = Directory.GetDirectories(BacktestOptionsMonitor.CurrentValue.Dir);
            bots.TryAddRange(botDirs.Select(d => Path.GetFileName(d)));

            foreach (var botDir in botDirs)
            {
                var bot = Path.GetFileName(botDir);
                if (botName != null && bot != botName) { continue; }

                var symbolDirs = Directory.GetDirectories(botDir);
                symbols.TryAddRange(symbolDirs.Select(d => Path.GetFileName(d)));

                foreach (var symbolDir in symbolDirs)
                {
                    var symbolName = Path.GetFileName(symbolDir);
                    symbols.TryAdd(SymbolNameNormalizer.Normalize(symbolName));

                    foreach (var timeFrameDir in Directory.GetDirectories(symbolDir))
                    {
                        var timeFrame = Path.GetFileName(timeFrameDir);
                        var dateSpanDirs = Directory.GetDirectories(timeFrameDir);
                        foreach (var dateSpanDir in dateSpanDirs)
                        {
                            var split = Path.GetFileName(dateSpanDir).Split('-');
                            if (split.Length != 2) continue;
                            var from = DateOnly.ParseExact(split[0], "yyyy.MM.dd");
                            var to = DateOnly.ParseExact(split[1], "yyyy.MM.dd");

                            var run = new OptimizationRun
                            {
                                Id = new OptimizationRunId
                                {
                                    Bot = bot,
                                    Symbol = symbolName,
                                    TimeFrame = timeFrame,
                                    Start = split[0],
                                    End = split[1],
                                },
                            };

                            runs.Add(run);
                        }
                    }
                }
            }

            Bots = bots;
            Markets = symbols;

            return runs;
        });
    }

    #endregion

    public Task<IEnumerable<string>> GetSymbols()
    {
        //foreach (var machineDir in Directory.GetDirectories(ResultsDir))
        //{
        //    foreach (var symbolNameDir in Directory.GetDirectories(machineDir))
        //    {
        //        var symbolName = Path.GetFileName(symbolNameDir);
        //        SymbolsAvailable.Add(symbolName);
        //    }
        //}
        //return Task.FromResult<IEnumerable<string>>(new string[] { "TODO" });
        return Task.Run<IEnumerable<string>>(() =>
        {
            var result = new HashSet<string>();

            foreach (var botDir in Directory.GetDirectories(BacktestOptionsMonitor.CurrentValue.Dir))
            {
                result.TryAddRange(Directory.GetDirectories(botDir).Select(d => SymbolNameNormalizer.Normalize(Path.GetFileName(d))));
            }
            return result;
        });
    }

    public Task<IEnumerable<string>> GetBots()
        => Task.Run<IEnumerable<string>>(() =>
            Directory.GetDirectories(BacktestOptionsMonitor.CurrentValue.Dir).Select(d => Path.GetFileName(d))
        );

    #endregion

    #region (Private) Methods

    private void PopulateParametersFromStream(string json, OptimizationParameters p)
    {
        JsonDocument doc;

        doc = JsonDocument.Parse(json);
        //try
        //{
        //}
        //catch (JsonException ex)
        //{
        //    if (ex.Message.Contains("'N' is an invalid start of a value."))
        //    {
        //        stream.Seek(0, SeekOrigin.Begin);
        //        var unfixedJson = stream.ReadAllText();
        //        doc = JsonDocument.Parse(JsonCompatibility.Fix(unfixedJson));
        //        //doc = JsonDocument.Parse(JsonCompatibility.Fix(unfixedJson), new JsonDocumentOptions( ));
        //    }
        //    else { throw; }
        //}

        var config = doc.RootElement.GetProperty("Config");

        foreach (var child in config.EnumerateObject())
        {
            //Console.WriteLine($"{child.Name} = {child.Value}");
            if (!p.Parameters.ContainsKey(child.Name))
            {
                p.Parameters.Add(child.Name, new BacktestParameter() { Name = child.Name });
            }
            var parameter = p.Parameters[child.Name];

            if (parameter.Type == null)
            {
                if (child.Value.ValueKind == JsonValueKind.String)
                {
                    parameter.Type = typeof(string);
                    parameter.Values = new();
                }
                else if (child.Value.ValueKind == JsonValueKind.Number && child.Value.TryGetDecimal(out var i))
                {
                    parameter.Type = typeof(decimal);
                    parameter.Min = i;
                    parameter.Max = i;
                }
                else if (child.Value.ValueKind == JsonValueKind.True || child.Value.ValueKind == JsonValueKind.False)
                {
                    parameter.Type = typeof(bool);
                }
            }

            if (parameter.Type != null)
            {
                switch (parameter.Type.ToString())
                {
                    case "System.String":
                        var s = child.Value.GetString();
                        if (s != null) { parameter.Values.TryAdd(s); }
                        break;
                    case "System.Decimal":
                        if (child.Value.TryGetDecimal(out var i))
                        {
                            parameter.Min = Math.Min(i, parameter.Min);
                            parameter.Max = Math.Max(i, parameter.Max);
                        }
                        break;
                    case "System.Boolean":
                        if (child.Value.ValueKind == JsonValueKind.True)
                        {
                            parameter.HasTrue = true;
                        }
                        if (child.Value.ValueKind == JsonValueKind.False)
                        {
                            parameter.HasFalse = true;
                        }
                        break;
                    default:
                        Debug.WriteLine("Unknown parameter type: " + parameter?.Type);
                        break;
                }
            }

        }

    }

    #endregion

}

#if false // Imported

public Task<IEnumerable<OptimizationRunId>> GetRuns()
    {
        return Task.Run<IEnumerable<OptimizationRunId>>(() =>
        {
            var runs = new List<OptimizationRunId>();

            var markets = new HashSet<string>();
            var bots = new HashSet<string>();

            foreach (var marketDir in Directory.GetDirectories(OptimizationResultsConfig.CurrentValue.BacktestsDir))
            {
                var symbolName = Path.GetFileName(marketDir);
                markets.TryAdd(CleanMarketDir(symbolName));

                var botDirs = Directory.GetDirectories(marketDir);
                bots.TryAddRange(botDirs.Select(d => Path.GetFileName(d)));

                foreach (var botDir in botDirs)
                {
                    var bot = Path.GetFileName(botDir);
                    foreach (var timeFrameDir in Directory.GetDirectories(botDir))
                    {
                        var timeFrame = Path.GetFileName(timeFrameDir);
                        var dateSpanDirs = Directory.GetDirectories(timeFrameDir);
                        foreach (var dateSpanDir in dateSpanDirs)
                        {
                            var split = Path.GetFileName(dateSpanDir).Split('-');
                            if (split.Length != 2) continue;
                            var from = DateOnly.ParseExact(split[0], "yyyy.MM.dd");
                            var to = DateOnly.ParseExact(split[1], "yyyy.MM.dd");

                            runs.Add(new OptimizationRunId
                            {
                                Bot = bot,
                                Symbol = symbolName,
                                TimeFrame = timeFrame,
                                Start = split[0],
                                End = split[1],
                            });
                        }
                    }
                }
            }

            Bots = bots;
            Markets = markets;

            return runs;
        });
    }

    public Task<IEnumerable<string>> GetBots()
    {
        //var machines = new List<string>();
        //machines.AddRange(Directory.GetDirectories(root).Select(d => Path.GetFileName(d));
        //var markets = new HashSet<string>();

        return Task.Run<IEnumerable<string>>(() =>
        {
            var bots = new HashSet<string>();

            foreach (var machineDir in Directory.GetDirectories(OptimizationResultsConfig.CurrentValue.BacktestsDir))
            {
                //var machineDir = Path.Combine(root, machine);
                //markets.TryAddRange(Directory.GetDirectories(machineDir).Select(d => CleanMarketDir(Path.GetFileName(d))));

                foreach (var marketDir in Directory.GetDirectories(machineDir))
                {
                    bots.TryAddRange(Directory.GetDirectories(marketDir).Select(d => CleanMarketDir(Path.GetFileName(d))));
                }
            }
            return bots;
        });
    }
#endif
