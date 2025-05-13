using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using System.IO;
using LionFire.Threading;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace LionFire.Trading.Backtesting;

public class BacktestFileMigrator : IHostedService
{
    IngestOptions InjestConfig;
    public IOptionsMonitor<IngestOptions> InjestConfigMonitor { get; }
    public ILogger<BacktestFileMigrator> Logger { get; }

    public BacktestFileMigrator(IOptionsMonitor<IngestOptions> backtestConfig, ILogger<BacktestFileMigrator> logger)
    {
        InjestConfigMonitor = backtestConfig;
        Logger = logger;
    }

    string BacktestPath;
    Timer PollTimer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!InjestConfigMonitor.CurrentValue.Enabled)
        {
            Logger.LogDebug("InjestConfig.Enabled is false.  Not starting.");
            return Task.CompletedTask;
        }

        InjestConfig = InjestConfigMonitor.CurrentValue;

        BacktestPath = InjestConfig.BacktestsRoot_Old;

        if (!(InjestConfig.MultiMachineResultDirs?.Count > 0))
        {
            Logger.LogWarning($"Injest:MultiMachineResultDirs is not set to an array of dirs.  Not starting injestion of backtest files.");
            return Task.CompletedTask;
        }

        PollTimer = new Timer(10 * 60 * 1000);
        PollTimer.Elapsed += PollTimer_Elapsed;
        Task.Run(() => { MigrateToDateDirs(retryErrors: true); PollTimer.Enabled = true; }).FireAndForget();

        return Task.CompletedTask;
    }

    private void PollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => MigrateToDateDirs();

    const string DateFormat = "yyyy.MM.dd";

    private void MigrateToDateDirs(bool retryErrors = false)
    {
        var sw = Stopwatch.StartNew();

        int count = 0;
        int dirCount = 0;

        string targetDir = null;

        //var jsonOptions = new JsonSerializerOptions
        //{
        //    NumberHandling =
        //    //JsonNumberHandling.AllowReadingFromString |
        //    JsonNumberHandling.AllowNamedFloatingPointLiterals
        //};

        if (Directory.Exists(InjestConfig.BacktestsRoot_Old))
        {
            Logger.LogTrace($"Migrating backtest files from {InjestConfig.BacktestsRoot_Old}");
            foreach (var marketDir in Directory.GetDirectories(InjestConfig.BacktestsRoot_Old))
            {
                var market = Path.GetFileName(marketDir);
                foreach (var botDir in Directory.GetDirectories(marketDir))
                {
                    var bot = Path.GetFileName(botDir);
                    foreach (var timeFrameDir in Directory.GetDirectories(botDir))
                    {
                        var timeFrame = Path.GetFileName(timeFrameDir);

                        if (retryErrors)
                        {
                            var errorDir = Path.Combine(timeFrameDir, "Error");
                            if (Directory.Exists(errorDir))
                            {
                                if (_InjestFromTimeFrameDir(errorDir, isRetry: true))
                                {
                                    if (Directory.GetFiles(errorDir).Length == 0 && Directory.GetDirectories(errorDir).Length == 0)
                                    {
                                        Directory.Delete(errorDir);
                                    }
                                }
                            }
                        }

                        _InjestFromTimeFrameDir(timeFrameDir);

                        bool _InjestFromTimeFrameDir(string timeFrameDir, bool isRetry = false)
                        {
                            dirCount++;

                            bool allSucceeded = true;
                            foreach (var file in Directory.GetFiles(timeFrameDir))
                            {
                                try
                                {
                                    if (!file.EndsWith(".json") || file.EndsWith(".trades.json")) { continue; }
                                    var json = File.ReadAllText(file);
                                    var backtest = JsonConvert.DeserializeObject<BacktestResult>(json);
                                    //var backtest = System.Text.Json.JsonSerializer.Deserialize<BacktestResult>(json, jsonOptions);
                                    var startString = backtest.Start.HasValue ? backtest.Start.Value.ToString(DateFormat) : "_";
                                    var endString = backtest.End.HasValue ? backtest.End.Value.ToString(DateFormat) : "_";

                                    targetDir = Path.Combine(InjestConfig.BacktestsRoot_Old, market, bot, timeFrame, $"{startString}-{endString}");
                                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                                    File.Move(file, Path.Combine(targetDir, Path.GetFileName(file)));

                                    var tradesFile = file.Replace(".json", ".trades.json");
                                    if (File.Exists(tradesFile))
                                    {
                                        File.Move(tradesFile, Path.Combine(targetDir, Path.GetFileName(tradesFile)));
                                    }
                                    count++;
                                }
                                catch (Exception ex)
                                {
                                    allSucceeded = false;
                                    Logger.LogError(ex, $"Failed to move '{file}' to {targetDir}");
                                    if (!isRetry)
                                    {
                                        try
                                        {
                                            targetDir = Path.Combine(InjestConfig.BacktestsRoot_Old, market, bot, timeFrame, $"Error");
                                            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                                            File.Move(file, Path.Combine(targetDir, Path.GetFileName(file)));

                                            var tradesFile = file.Replace(".json", ".trades.json");
                                            if (File.Exists(tradesFile))
                                            {
                                                File.Move(tradesFile, Path.Combine(targetDir, Path.GetFileName(tradesFile)));
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                            return allSucceeded;
                        }
                    }
                }
            }
            var msg = $"BacktestFileMigrator moved {count} files from {dirCount} directories in {sw.ElapsedMilliseconds}ms";
            if (sw.ElapsedMilliseconds > 3 * 1000)
            {
                Logger.LogWarning(msg);
            }
            else if (count > 0)
            {
                Logger.LogInformation(msg);
            }
            else
            {
                Logger.LogInformation(msg);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (PollTimer != null)
        {
            PollTimer.Enabled = false;
            PollTimer.Dispose();
            PollTimer = null;
        }
        return Task.CompletedTask;
    }
}
