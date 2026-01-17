using JasperFx.CommandLine;
using Humanizer;
using LionFire.Hosting;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators;
using LionFire.Trading.Optimization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Cli.Optimization;

#region Input Classes

public class OptimizeRunInput : CommonTradingInput
{
    [Description("Bot type to optimize (e.g., PAtrBot, PLorentzianBot)")]
    [FlagAlias("bot", 'b')]
    public string? BotType { get; set; }

    [Description("HJSON/JSON config file with optimization parameters")]
    [FlagAlias("config", 'c')]
    public string? ConfigFile { get; set; }

    [Description("Progress update interval in seconds (default: 5)")]
    [FlagAlias("progress-interval", 'p')]
    public int ProgressIntervalSeconds { get; set; } = 5;

    [Description("Output file path for results (default: auto-generated)")]
    [FlagAlias("output", 'o')]
    public string? OutputFile { get; set; }

    [Description("Output format: hjson, json, csv (default: hjson)")]
    [FlagAlias("format")]
    public string OutputFormat { get; set; } = "hjson";

    [Description("Output progress as JSON lines (machine-parseable)")]
    [FlagAlias("json")]
    public bool JsonOutput { get; set; }

    [Description("Maximum number of backtests to run")]
    [FlagAlias("max-backtests", 'm')]
    public long MaxBacktests { get; set; } = 50000;

    [Description("Batch size for backtest execution")]
    [FlagAlias("batch-size")]
    public int BatchSize { get; set; } = 1024;

    [Description("Minimum parameter priority to include (lower = higher priority)")]
    [FlagAlias("min-priority")]
    public int MinParameterPriority { get; set; } = 0;

    [Description("Suppress progress output (only show final result)")]
    [FlagAlias("quiet", 'q')]
    public bool Quiet { get; set; }
}

#endregion

/// <summary>
/// In-process optimization command that runs without requiring Orleans Silo.
/// Designed for agent/CLI usage with progressive progress updates.
/// NOTE: This JasperFx command is not currently enabled. Use the handler-based command instead via Program.cs.
/// </summary>
[Area("optimize")]
[Description("Run optimization in-process with progress updates", Name = "run")]
public class OptimizeRunCommand : JasperFxAsyncCommand<OptimizeRunInput>
{
    public override async Task<bool> Execute(OptimizeRunInput input)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Build host with required services for optimization
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddEnvironmentVariables("DOTNET_");
                    config.AddJsonFile("appsettings.json", optional: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddSingleton<IndicatorProvider>()
                        .AutomationModel(context.Configuration)
                        .Automation(context.Configuration)
                        .AddHistoricalBars(context.Configuration)
                        .AddIndicators();
                })
                .Build();
            var serviceProvider = host.Services;

            // Resolve bot type
            var botTypeRegistry = serviceProvider.GetRequiredService<BotTypeRegistry>();
            Type? pBotType = null;

            if (!string.IsNullOrEmpty(input.ConfigFile))
            {
                // Load from config file
                if (!File.Exists(input.ConfigFile))
                {
                    return WriteError(input, $"Config file not found: {input.ConfigFile}");
                }

                // TODO: Load PMultiSim from HJSON/JSON config
                return WriteError(input, "Config file loading not yet implemented. Use command-line parameters.");
            }
            else if (!string.IsNullOrEmpty(input.BotType))
            {
                // Resolve bot type by name
                pBotType = botTypeRegistry.GetPBotType(input.BotType);
                if (pBotType == null)
                {
                    // Try with 'P' prefix
                    pBotType = botTypeRegistry.GetPBotType("P" + input.BotType);
                }

                if (pBotType == null)
                {
                    return WriteError(input, $"Bot type not found: {input.BotType}. Available types: {string.Join(", ", botTypeRegistry.PBotRegistry.Names.Keys)}");
                }

                // Close generic type if needed
                if (pBotType.IsGenericTypeDefinition)
                {
                    var typeArgs = pBotType.GetGenericArguments();
                    var closedArgs = typeArgs.Select(_ => typeof(double)).ToArray();
                    pBotType = pBotType.MakeGenericType(closedArgs);
                }
            }
            else
            {
                return WriteError(input, "Either --bot or --config must be specified.");
            }

            // Create optimization parameters
            var timeFrame = TimeFrame.Parse(input.IntervalFlag);
            var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(
                input.ExchangeFlag,
                input.ExchangeAreaFlag,
                input.Symbol,
                timeFrame
            );

            var pMultiSim = new PMultiSim
            {
                PBotType = pBotType,
                ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
                Start = input.FromFlag,
                EndExclusive = input.ToFlag,
            };

            // Create and configure optimization parameters
            pMultiSim.POptimization = new POptimization(pMultiSim)
            {
                MaxBacktests = input.MaxBacktests,
                MaxBatchSize = input.BatchSize,
                MinParameterPriority = input.MinParameterPriority,
            };

            // Write initial status
            WriteProgress(input, new ProgressInfo
            {
                Status = "starting",
                Timestamp = DateTimeOffset.UtcNow,
                Bot = input.BotType!,
                Symbol = input.Symbol,
                TimeFrame = input.IntervalFlag,
                StartDate = input.FromFlag.ToString("yyyy-MM-dd"),
                EndDate = input.ToFlag.ToString("yyyy-MM-dd"),
            });

            // Create and start optimization task
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                WriteProgress(input, new ProgressInfo
                {
                    Status = "cancelling",
                    Timestamp = DateTimeOffset.UtcNow,
                    Message = "Received interrupt signal, cancelling..."
                });
            };

            var optimizationTask = new OptimizationTask(serviceProvider, pMultiSim);
            optimizationTask.AddCancellationToken(cts.Token);

            await optimizationTask.StartAsync(cts.Token);

            // Progress reporting loop
            var progressInterval = TimeSpan.FromSeconds(input.ProgressIntervalSeconds);
            var lastProgressTime = DateTimeOffset.UtcNow;
            long lastCompleted = 0;

            while (optimizationTask.RunTask != null && !optimizationTask.RunTask.IsCompleted)
            {
                var delay = Task.Delay(progressInterval, cts.Token);
                var completedTask = await Task.WhenAny(optimizationTask.RunTask, delay);

                if (completedTask == optimizationTask.RunTask)
                {
                    break; // Optimization completed
                }

                // Report progress
                if (!input.Quiet && optimizationTask.Progress != null)
                {
                    var progress = optimizationTask.Progress;
                    var now = DateTimeOffset.UtcNow;
                    var elapsed = now - startTime;
                    var completed = progress.Completed;
                    var queued = progress.Queued;
                    var total = queued > 0 ? queued : completed;
                    var percent = total > 0 ? (double)completed / total * 100 : 0;

                    // Calculate rate and ETA
                    var timeSinceLastProgress = now - lastProgressTime;
                    var recentCompleted = completed - lastCompleted;
                    var rate = timeSinceLastProgress.TotalSeconds > 0
                        ? recentCompleted / timeSinceLastProgress.TotalSeconds
                        : 0;
                    var remaining = total - completed;
                    var eta = rate > 0 ? TimeSpan.FromSeconds(remaining / rate) : (TimeSpan?)null;

                    WriteProgress(input, new ProgressInfo
                    {
                        Status = "running",
                        Timestamp = now,
                        Percent = Math.Round(percent, 1),
                        Completed = completed,
                        Total = total,
                        Queued = queued,
                        Rate = Math.Round(rate, 1),
                        Elapsed = elapsed.Humanize(2),
                        Eta = eta?.Humanize(2) ?? "calculating..."
                    });

                    lastProgressTime = now;
                    lastCompleted = completed;
                }
            }

            // Wait for completion
            if (optimizationTask.RunTask != null)
            {
                await optimizationTask.RunTask;
            }

            stopwatch.Stop();

            // Check for cancellation
            if (cts.Token.IsCancellationRequested)
            {
                WriteProgress(input, new ProgressInfo
                {
                    Status = "cancelled",
                    Timestamp = DateTimeOffset.UtcNow,
                    Elapsed = stopwatch.Elapsed.Humanize(2),
                    Message = "Optimization cancelled by user"
                });
                return false;
            }

            // Get final results
            var finalProgress = optimizationTask.Progress ?? new OptimizationProgress();
            var outputDir = optimizationTask.OptimizationDirectory;

            // Write final status
            WriteProgress(input, new ProgressInfo
            {
                Status = "completed",
                Timestamp = DateTimeOffset.UtcNow,
                Percent = 100,
                Completed = finalProgress.Completed,
                Total = finalProgress.Queued > 0 ? finalProgress.Queued : finalProgress.Completed,
                Elapsed = stopwatch.Elapsed.Humanize(2),
                OutputDirectory = outputDir,
                Message = $"Optimization completed. Results in: {outputDir}"
            });

            return true;
        }
        catch (OperationCanceledException)
        {
            WriteProgress(input, new ProgressInfo
            {
                Status = "cancelled",
                Timestamp = DateTimeOffset.UtcNow,
                Elapsed = stopwatch.Elapsed.Humanize(2),
                Message = "Optimization cancelled"
            });
            return false;
        }
        catch (Exception ex)
        {
            WriteProgress(input, new ProgressInfo
            {
                Status = "failed",
                Timestamp = DateTimeOffset.UtcNow,
                Elapsed = stopwatch.Elapsed.Humanize(2),
                Error = ex.Message,
                Message = $"Optimization failed: {ex.Message}"
            });

            if (!input.JsonOutput)
            {
                AnsiConsole.WriteException(ex);
            }

            return false;
        }
    }

    private bool WriteError(OptimizeRunInput input, string message)
    {
        if (input.JsonOutput)
        {
            var error = new { status = "error", error = message, timestamp = DateTimeOffset.UtcNow };
            Console.WriteLine(JsonSerializer.Serialize(error, JsonOptions));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error: {message}[/]");
        }
        return false;
    }

    private void WriteProgress(OptimizeRunInput input, ProgressInfo progress)
    {
        if (input.Quiet && progress.Status == "running") return;

        if (input.JsonOutput)
        {
            Console.WriteLine(JsonSerializer.Serialize(progress, JsonOptions));
        }
        else
        {
            var timestamp = progress.Timestamp.ToString("HH:mm:ss");

            switch (progress.Status)
            {
                case "starting":
                    AnsiConsole.MarkupLine($"[blue][{timestamp}][/] Starting optimization: [cyan]{progress.Bot}[/] on [yellow]{progress.Symbol}[/] {progress.TimeFrame}");
                    AnsiConsole.MarkupLine($"         Date range: {progress.StartDate} to {progress.EndDate}");
                    break;

                case "running":
                    AnsiConsole.MarkupLine(
                        $"[blue][{timestamp}][/] Progress: [green]{progress.Percent}%[/] | " +
                        $"Completed: [cyan]{progress.Completed}/{progress.Total}[/] | " +
                        $"Rate: [yellow]{progress.Rate}/s[/] | " +
                        $"ETA: [magenta]{progress.Eta}[/]"
                    );
                    break;

                case "completed":
                    AnsiConsole.MarkupLine($"[green][{timestamp}][/] [bold green]Completed![/] {progress.Completed} backtests in {progress.Elapsed}");
                    if (!string.IsNullOrEmpty(progress.OutputDirectory))
                    {
                        AnsiConsole.MarkupLine($"         Results: [link]{progress.OutputDirectory}[/]");
                    }
                    break;

                case "cancelled":
                    AnsiConsole.MarkupLine($"[yellow][{timestamp}][/] Cancelled after {progress.Elapsed}");
                    break;

                case "failed":
                    AnsiConsole.MarkupLine($"[red][{timestamp}][/] [bold red]Failed:[/] {progress.Error}");
                    break;

                case "cancelling":
                    AnsiConsole.MarkupLine($"[yellow][{timestamp}][/] {progress.Message}");
                    break;
            }
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private class ProgressInfo
    {
        public string Status { get; set; } = "";
        public DateTimeOffset Timestamp { get; set; }
        public string? Bot { get; set; }
        public string? Symbol { get; set; }
        public string? TimeFrame { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public double? Percent { get; set; }
        public long? Completed { get; set; }
        public long? Total { get; set; }
        public long? Queued { get; set; }
        public double? Rate { get; set; }
        public string? Elapsed { get; set; }
        public string? Eta { get; set; }
        public string? OutputDirectory { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
