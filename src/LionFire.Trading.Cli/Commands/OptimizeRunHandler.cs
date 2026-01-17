using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.CommandLine;
using Humanizer;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using LionFire.Hosting;
using LionFire.Hosting.CommandLine;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Optimization;
using LionFire.Trading.HistoricalData;

namespace LionFire.Trading.Cli.Commands;

public static class OptimizeRunHandler
{
    /// <summary>
    /// Configures command options and adds them to the command
    /// </summary>
    public static void ConfigureCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        var botOption = new Option<string?>("--bot", "Bot type to optimize (e.g., PAtrBot)") { IsRequired = true };
        botOption.AddAlias("-b");
        cmd.AddOption(botOption);

        var symbolOption = new Option<string>("--symbol", () => "BTCUSDT", "Trading symbol");
        symbolOption.AddAlias("-s");
        cmd.AddOption(symbolOption);

        var exchangeOption = new Option<string>("--exchange", () => "Binance", "Exchange name");
        exchangeOption.AddAlias("-e");
        cmd.AddOption(exchangeOption);

        var areaOption = new Option<string>("--area", () => "futures", "Exchange area (spot, futures)");
        areaOption.AddAlias("-a");
        cmd.AddOption(areaOption);

        var timeframeOption = new Option<string>("--timeframe", () => "h1", "Timeframe (m1, m5, h1, etc.)");
        timeframeOption.AddAlias("-t");
        cmd.AddOption(timeframeOption);

        var fromOption = new Option<DateTime>("--from", () => DateTime.UtcNow.AddMonths(-1), "Start date");
        fromOption.AddAlias("-f");
        cmd.AddOption(fromOption);

        cmd.AddOption(new Option<DateTime>("--to", () => DateTime.UtcNow, "End date"));

        var progressIntervalOption = new Option<int>("--progress-interval", () => 5, "Progress update interval in seconds");
        progressIntervalOption.AddAlias("-p");
        cmd.AddOption(progressIntervalOption);

        cmd.AddOption(new Option<bool>("--json", () => false, "Output progress as JSON lines (machine-parseable)"));

        var quietOption = new Option<bool>("--quiet", () => false, "Suppress progress output");
        quietOption.AddAlias("-q");
        cmd.AddOption(quietOption);

        var maxBacktestsOption = new Option<long>("--max-backtests", () => 50000, "Maximum backtests to run");
        maxBacktestsOption.AddAlias("-m");
        cmd.AddOption(maxBacktestsOption);

        cmd.AddOption(new Option<int>("--batch-size", () => 1024, "Batch size for execution"));
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Run =>
        (context, builder) =>
        {
            // Configure services needed for optimization
            builder.Services
                .AutomationModel(builder.Configuration) // Add BotTypeRegistry and related services
                .Automation(builder.Configuration)
                .AddHistoricalBars(builder.Configuration)

                // Register bot types (needed for BotTypeRegistry)
                .RegisterTypesFromAssemblies<IPBot2>(typeof(PAtrBot<>))
                .RegisterTypesFromAssemblies<IBot2>(typeof(AtrBot<>))

                // Register Binance client for historical data retrieval
                .AddSingleton<IBinanceRestClient>(sp => new BinanceRestClient());

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                // Get options from context
                var options = context.TryGetOptions<OptimizeRunOptions>();

                // If options not bound, try parsing from command line directly
                if (options == null)
                {
                    var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                    options = ParseArguments(args);
                }

                try
                {
                    // Validate bot type
                    if (string.IsNullOrEmpty(options.Bot))
                    {
                        WriteError(options.Json, "Bot type is required. Use --bot <type>");
                        return;
                    }

                    // Resolve bot type
                    var botTypeRegistry = serviceProvider.GetRequiredService<BotTypeRegistry>();
                    Type? pBotType = botTypeRegistry.GetPBotType(options.Bot);

                    if (pBotType == null)
                    {
                        // Try with 'P' prefix
                        pBotType = botTypeRegistry.GetPBotType("P" + options.Bot);
                    }

                    if (pBotType == null)
                    {
                        var availableTypes = string.Join(", ", botTypeRegistry.PBotRegistry.Names.Keys.Take(10));
                        WriteError(options.Json, $"Bot type not found: {options.Bot}. Available: {availableTypes}...");
                        return;
                    }

                    // Close generic type if needed
                    if (pBotType.IsGenericTypeDefinition)
                    {
                        var typeArgs = pBotType.GetGenericArguments();
                        var closedArgs = typeArgs.Select(_ => typeof(double)).ToArray();
                        pBotType = pBotType.MakeGenericType(closedArgs);
                    }

                    // Create optimization parameters
                    var timeFrame = TimeFrame.Parse(options.Timeframe);
                    var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(options.Exchange, options.Area, options.Symbol, timeFrame);

                    var pMultiSim = new PMultiSim
                    {
                        PBotType = pBotType,
                        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
                        Start = options.From,
                        EndExclusive = options.To,
                    };

                    pMultiSim.POptimization = new POptimization(pMultiSim)
                    {
                        MaxBacktests = options.MaxBacktests,
                        MaxBatchSize = options.BatchSize,
                    };

                    // Write starting status
                    WriteProgress(options.Json, options.Quiet, new ProgressInfo
                    {
                        Status = "starting",
                        Timestamp = DateTimeOffset.UtcNow,
                        Bot = options.Bot,
                        Symbol = options.Symbol,
                        TimeFrame = options.Timeframe,
                        StartDate = options.From.ToString("yyyy-MM-dd"),
                        EndDate = options.To.ToString("yyyy-MM-dd"),
                    });

                    // Setup cancellation
                    using var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        WriteProgress(options.Json, options.Quiet, new ProgressInfo
                        {
                            Status = "cancelling",
                            Timestamp = DateTimeOffset.UtcNow,
                            Message = "Received interrupt signal, cancelling..."
                        });
                    };

                    // Create and start optimization
                    var optimizationTask = new OptimizationTask(serviceProvider, pMultiSim);
                    optimizationTask.AddCancellationToken(cts.Token);

                    await optimizationTask.StartAsync(cts.Token);

                    // Progress loop
                    var progressIntervalTs = TimeSpan.FromSeconds(options.ProgressInterval);
                    var lastProgressTime = DateTimeOffset.UtcNow;
                    long lastCompleted = 0;

                    while (optimizationTask.RunTask != null && !optimizationTask.RunTask.IsCompleted)
                    {
                        var delay = Task.Delay(progressIntervalTs, cts.Token);
                        var completedTask = await Task.WhenAny(optimizationTask.RunTask, delay);

                        if (completedTask == optimizationTask.RunTask) break;

                        // Report progress
                        if (!options.Quiet && optimizationTask.Progress != null)
                        {
                            var progress = optimizationTask.Progress;
                            var now = DateTimeOffset.UtcNow;
                            var elapsed = now - startTime;
                            var completed = progress.Completed;
                            var queued = progress.Queued;
                            var total = queued > 0 ? queued : completed;
                            var percent = total > 0 ? (double)completed / total * 100 : 0;

                            var timeSinceLastProgress = now - lastProgressTime;
                            var recentCompleted = completed - lastCompleted;
                            var rate = timeSinceLastProgress.TotalSeconds > 0
                                ? recentCompleted / timeSinceLastProgress.TotalSeconds
                                : 0;
                            var remaining = total - completed;
                            var eta = rate > 0 ? TimeSpan.FromSeconds(remaining / rate) : (TimeSpan?)null;

                            WriteProgress(options.Json, options.Quiet, new ProgressInfo
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

                    if (cts.Token.IsCancellationRequested)
                    {
                        WriteProgress(options.Json, options.Quiet, new ProgressInfo
                        {
                            Status = "cancelled",
                            Timestamp = DateTimeOffset.UtcNow,
                            Elapsed = stopwatch.Elapsed.Humanize(2),
                            Message = "Optimization cancelled by user"
                        });
                        return;
                    }

                    var finalProgress = optimizationTask.Progress ?? new OptimizationProgress();
                    var outputDir = optimizationTask.OptimizationDirectory;

                    WriteProgress(options.Json, options.Quiet, new ProgressInfo
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
                }
                catch (OperationCanceledException)
                {
                    WriteProgress(options.Json, options.Quiet, new ProgressInfo
                    {
                        Status = "cancelled",
                        Timestamp = DateTimeOffset.UtcNow,
                        Elapsed = stopwatch.Elapsed.Humanize(2),
                        Message = "Optimization cancelled"
                    });
                }
                catch (Exception ex)
                {
                    WriteProgress(options.Json, options.Quiet, new ProgressInfo
                    {
                        Status = "failed",
                        Timestamp = DateTimeOffset.UtcNow,
                        Elapsed = stopwatch.Elapsed.Humanize(2),
                        Error = ex.Message,
                        Message = $"Optimization failed: {ex.Message}"
                    });

                    if (!options.Json)
                    {
                        AnsiConsole.WriteException(ex);
                    }
                }
            });
        };

    private static OptimizeRunOptions ParseArguments(string[] args)
    {
        var options = new OptimizeRunOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = i + 1 < args.Length ? args[i + 1] : null;

            switch (arg.ToLowerInvariant())
            {
                case "--bot":
                case "-b":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Bot = nextArg;
                        i++;
                    }
                    break;

                case "--symbol":
                case "-s":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Symbol = nextArg;
                        i++;
                    }
                    break;

                case "--exchange":
                case "-e":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Exchange = nextArg;
                        i++;
                    }
                    break;

                case "--area":
                case "-a":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Area = nextArg;
                        i++;
                    }
                    break;

                case "--timeframe":
                case "-t":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Timeframe = nextArg;
                        i++;
                    }
                    break;

                case "--from":
                case "-f":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (DateTime.TryParse(nextArg, out var from))
                        {
                            options.From = from;
                        }
                        i++;
                    }
                    break;

                case "--to":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (DateTime.TryParse(nextArg, out var to))
                        {
                            options.To = to;
                        }
                        i++;
                    }
                    break;

                case "--progress-interval":
                case "-p":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (int.TryParse(nextArg, out var interval))
                        {
                            options.ProgressInterval = interval;
                        }
                        i++;
                    }
                    break;

                case "--json":
                    options.Json = true;
                    break;

                case "--quiet":
                case "-q":
                    options.Quiet = true;
                    break;

                case "--max-backtests":
                case "-m":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (long.TryParse(nextArg, out var maxBacktests))
                        {
                            options.MaxBacktests = maxBacktests;
                        }
                        i++;
                    }
                    break;

                case "--batch-size":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (int.TryParse(nextArg, out var batchSize))
                        {
                            options.BatchSize = batchSize;
                        }
                        i++;
                    }
                    break;
            }
        }

        return options;
    }

    private static void WriteError(bool jsonOutput, string message)
    {
        if (jsonOutput)
        {
            var error = new { status = "error", error = message, timestamp = DateTimeOffset.UtcNow };
            Console.WriteLine(JsonSerializer.Serialize(error, JsonOptions));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error: {message}[/]");
        }
    }

    private static void WriteProgress(bool jsonOutput, bool quiet, ProgressInfo progress)
    {
        if (quiet && progress.Status == "running") return;

        if (jsonOutput)
        {
            Console.WriteLine(JsonSerializer.Serialize(progress, JsonOptions));
        }
        else
        {
            var timestamp = progress.Timestamp.ToString("HH:mm:ss");

            switch (progress.Status)
            {
                case "starting":
                    AnsiConsole.MarkupLine($"[blue][[{timestamp}]][/] Starting optimization: [cyan]{progress.Bot}[/] on [yellow]{progress.Symbol}[/] {progress.TimeFrame}");
                    AnsiConsole.MarkupLine($"         Date range: {progress.StartDate} to {progress.EndDate}");
                    break;

                case "running":
                    AnsiConsole.MarkupLine(
                        $"[blue][[{timestamp}]][/] Progress: [green]{progress.Percent}%[/] | " +
                        $"Completed: [cyan]{progress.Completed}/{progress.Total}[/] | " +
                        $"Rate: [yellow]{progress.Rate}/s[/] | " +
                        $"ETA: [magenta]{progress.Eta}[/]"
                    );
                    break;

                case "completed":
                    AnsiConsole.MarkupLine($"[green][[{timestamp}]][/] [bold green]Completed![/] {progress.Completed} backtests in {progress.Elapsed}");
                    if (!string.IsNullOrEmpty(progress.OutputDirectory))
                    {
                        AnsiConsole.MarkupLine($"         Results: [link]{progress.OutputDirectory}[/]");
                    }
                    break;

                case "cancelled":
                    AnsiConsole.MarkupLine($"[yellow][[{timestamp}]][/] Cancelled after {progress.Elapsed}");
                    break;

                case "failed":
                    AnsiConsole.MarkupLine($"[red][[{timestamp}]][/] [bold red]Failed:[/] {progress.Error}");
                    break;

                case "cancelling":
                    AnsiConsole.MarkupLine($"[yellow][[{timestamp}]][/] {progress.Message}");
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
