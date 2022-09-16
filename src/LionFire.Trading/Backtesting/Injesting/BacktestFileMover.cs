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

namespace LionFire.Trading.Backtesting;

public class BacktestFileMover : IHostedService
{
    InjestOptions InjestConfig;
    public IOptionsMonitor<InjestOptions> InjestConfigMonitor { get; }
    public ILogger<BacktestFileMover> Logger { get; }

    public BacktestFileMover(IOptionsMonitor<InjestOptions> backtestConfig, ILogger<BacktestFileMover> logger)
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

        if (!(InjestConfig.MultiMachineResultDirs?.Count > 0) /*&& !(InjestConfig.MarketsResultDirs?.Count > 0)*/)
        {
            Logger.LogWarning($"Injest:MultiMachineResultDirs is not set to an array of dirs.  Not starting injestion of backtest files.");
            return Task.CompletedTask;
        }

        PollTimer = new Timer(60 * 1000);
        PollTimer.Elapsed += PollTimer_Elapsed;
        Task.Run(() => { InjestBacktestFiles(); PollTimer.Enabled = true; }).FireAndForget();

        return Task.CompletedTask;
    }

    private void PollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => InjestBacktestFiles();
        
    private void InjestBacktestFiles()
    {
        var sw = Stopwatch.StartNew();

        int moveCount = 0;
        int dirCount = 0;

        Logger.LogTrace($"Injesting backtest files from {InjestConfig.MultiMachineResultDirs.AggregateOrDefault((x, y) => $"{x},{y}")}");
        foreach (var multiMachineResultDir in InjestConfig.MultiMachineResultDirs)
        {
            //Logger.LogTrace($"Injesting backtest files from {InjestConfig.MarketsResultDirs.AggregateOrDefault((x, y) => $"{x},{y}")}");

            foreach (var machineDir in Directory.GetDirectories(multiMachineResultDir)/*.Concat(InjestConfig.MarketsResultDirs)*/)
            {
                foreach (var marketDir in Directory.GetDirectories(machineDir))
                {
                    var market = Path.GetFileName(marketDir);
                    foreach (var botDir in Directory.GetDirectories(marketDir))
                    {
                        var bot = Path.GetFileName(botDir);
                        foreach (var timeFrameDir in Directory.GetDirectories(botDir))
                        {
                            var timeFrame = Path.GetFileName(timeFrameDir);
                            var targetDir = Path.Combine(InjestConfig.BacktestsRoot_Old, market, bot, timeFrame);

                            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                            dirCount++;
                            foreach (var file in Directory.GetFiles(timeFrameDir))
                            {
                                try
                                {
                                    File.Move(file, Path.Combine(targetDir, Path.GetFileName(file)));
                                    moveCount++;
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, $"Failed to move '{file}' to {targetDir}");
                                }
                            }

                            foreach (var datesDir in Directory.GetDirectories(timeFrameDir))
                            {
                                dirCount++;
                                var dates = Path.GetFileName(datesDir);
                                targetDir = Path.Combine(InjestConfig.BacktestsRoot_Old, market, bot, timeFrame, dates);

                                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                                foreach (var file in Directory.GetFiles(datesDir))
                                {
                                    try
                                    {
                                        File.Move(file, Path.Combine(targetDir, Path.GetFileName(file)));
                                        moveCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogError(ex, $"Failed to move '{file}' to {targetDir}");
                                    }
                                }
                                TryDeleteDir(datesDir);
                            }
                            TryDeleteDir(timeFrameDir);
                        }
                        TryDeleteDir(botDir);
                    }
                    TryDeleteDir(marketDir);
                }
                //if (!InjestConfig.MarketsResultDirs.Contains(machineDir))
                {
                    TryDeleteDir(machineDir);
                }
            }
        }
        var msg = $"InjestBacktestfiles injested {moveCount} files from {dirCount} directories in {sw.ElapsedMilliseconds}ms";
        if (sw.ElapsedMilliseconds > 3 * 1000)
        {
            Logger.LogWarning(msg);
        }
        else if (moveCount > 0)
        {
            Logger.LogInformation(msg);
        }
        else
        {
            Logger.LogInformation(msg);
        }
    }
    private void TryDeleteDir(string dir)
    {
        try
        {
            Directory.Delete(dir);
        }
        catch
        {
            Logger.LogInformation($"Cleanup failed to delete directory: {dir}");
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
