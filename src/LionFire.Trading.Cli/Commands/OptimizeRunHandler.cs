using Hjson;
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
using LionFire.Trading.Automation.Optimization.Scoring;
using LionFire.Trading.Optimization;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Journal;

namespace LionFire.Trading.Cli.Commands;

public static class OptimizeRunHandler
{
    /// <summary>
    /// Configures command options and adds them to the command
    /// </summary>
    public static void ConfigureCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        // Config file options
        var configOption = new Option<string?>("--config", "Path to HJSON/JSON config file");
        configOption.AddAlias("-c");
        cmd.AddOption(configOption);

        cmd.AddOption(new Option<string?>("--preset", "Named preset from presets directory"));

        // Core options
        var botOption = new Option<string?>("--bot", "Bot type to optimize (e.g., PAtrBot)");
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

        // Trade journal options
        var journalsOption = new Option<bool?>("--journals", "Enable trade journals (saves detailed trade data for top results)");
        journalsOption.AddAlias("-j");
        cmd.AddOption(journalsOption);

        cmd.AddOption(new Option<int?>("--keep-journals", "Number of top results to keep trade journals for (default: 5)"));
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

                // Always parse args ourselves to track which properties were explicitly set
                // System.CommandLine binding doesn't track this for us
                var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                var rawOptions = ParseArguments(args);

                // Load config file and presets, merging with command line options
                // Precedence: Command line > Config file > Preset > Defaults
                OptimizeRunOptions options;
                try
                {
                    options = OptimizeConfigLoader.LoadAndMerge(rawOptions, rawOptions.ExplicitlySetProperties);
                }
                catch (FileNotFoundException ex)
                {
                    WriteError(rawOptions.Json, ex.Message);
                    Environment.ExitCode = 1;
                    return;
                }

                try
                {
                    // Validate bot type
                    if (string.IsNullOrEmpty(options.Bot))
                    {
                        WriteError(options.Json, "Bot type is required. Use --bot <type>");
                        Environment.ExitCode = 1;
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
                        Environment.ExitCode = 1;
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

                    // Snap dates to bar boundaries to exclude incomplete bars
                    // Start: snap forward to next bar boundary (include only complete bars)
                    // EndExclusive: snap backward to previous bar boundary (exclude incomplete current bar)
                    var snappedStart = timeFrame.GetPeriodStart(options.From);
                    if (snappedStart < options.From)
                    {
                        snappedStart = timeFrame.AddBars(snappedStart, 1); // Move to next bar start
                    }
                    var snappedEnd = timeFrame.GetPeriodStart(options.To);

                    var pMultiSim = new PMultiSim
                    {
                        PBotType = pBotType,
                        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
                        Start = snappedStart,
                        EndExclusive = snappedEnd,
                    };

                    pMultiSim.POptimization = new POptimization(pMultiSim)
                    {
                        MaxBacktests = options.MaxBacktests,
                        MaxBatchSize = options.BatchSize,
                        TradeJournalOptions = new TradeJournalOptions
                        {
                            Enabled = options.Journals ?? false,
                            KeepTradeJournalsForTopNResults = options.KeepJournals ?? 5,
                        },
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
                        Environment.ExitCode = 130; // Standard exit code for SIGINT
                        return;
                    }

                    var finalProgress = optimizationTask.Progress ?? new OptimizationProgress();
                    var outputDir = optimizationTask.OptimizationDirectory;

                    // Calculate optimization score
                    OptimizationScore? score = null;
                    if (!string.IsNullOrEmpty(outputDir))
                    {
                        try
                        {
                            var backtestResults = BacktestResultsReader.ReadFromDirectory(outputDir);
                            if (backtestResults.Count > 0)
                            {
                                var scorer = new OptimizationScorer(backtestResults);
                                score = scorer.Calculate();

                                // Write score to file
                                var scoreFilePath = Path.Combine(outputDir, "OptimizationScore.hjson");
                                var scoreJson = JsonSerializer.Serialize(score, new JsonSerializerOptions
                                {
                                    WriteIndented = true,
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                                });
                                var hjson = Hjson.JsonValue.Parse(scoreJson).ToString(new Hjson.HjsonOptions { EmitRootBraces = false });
                                await File.WriteAllTextAsync(scoreFilePath, hjson);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Don't fail the completion if scoring fails
                            if (!options.Quiet)
                            {
                                AnsiConsole.MarkupLine($"[yellow]Warning: Failed to calculate score: {ex.Message}[/]");
                            }
                        }
                    }

                    WriteProgress(options.Json, options.Quiet, new ProgressInfo
                    {
                        Status = "completed",
                        Timestamp = DateTimeOffset.UtcNow,
                        Percent = 100,
                        Completed = finalProgress.Completed,
                        Total = finalProgress.Queued > 0 ? finalProgress.Queued : finalProgress.Completed,
                        Elapsed = stopwatch.Elapsed.Humanize(2),
                        OutputDirectory = outputDir,
                        Score = score,
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
                    Environment.ExitCode = 130; // Standard exit code for SIGINT
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
                    Environment.ExitCode = 1;
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
                case "--config":
                case "-c":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Config = nextArg;
                        i++;
                    }
                    break;

                case "--preset":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Preset = nextArg;
                        i++;
                    }
                    break;

                case "--bot":
                case "-b":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Bot = nextArg;
                        options.ExplicitlySetProperties.Add("Bot");
                        i++;
                    }
                    break;

                case "--symbol":
                case "-s":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Symbol = nextArg;
                        options.ExplicitlySetProperties.Add("Symbol");
                        i++;
                    }
                    break;

                case "--exchange":
                case "-e":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Exchange = nextArg;
                        options.ExplicitlySetProperties.Add("Exchange");
                        i++;
                    }
                    break;

                case "--area":
                case "-a":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Area = nextArg;
                        options.ExplicitlySetProperties.Add("Area");
                        i++;
                    }
                    break;

                case "--timeframe":
                case "-t":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Timeframe = nextArg;
                        options.ExplicitlySetProperties.Add("Timeframe");
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
                            options.ExplicitlySetProperties.Add("From");
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
                            options.ExplicitlySetProperties.Add("To");
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
                            options.ExplicitlySetProperties.Add("ProgressInterval");
                        }
                        i++;
                    }
                    break;

                case "--json":
                    options.Json = true;
                    options.ExplicitlySetProperties.Add("Json");
                    break;

                case "--quiet":
                case "-q":
                    options.Quiet = true;
                    options.ExplicitlySetProperties.Add("Quiet");
                    break;

                case "--max-backtests":
                case "-m":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (long.TryParse(nextArg, out var maxBacktests))
                        {
                            options.MaxBacktests = maxBacktests;
                            options.ExplicitlySetProperties.Add("MaxBacktests");
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
                            options.ExplicitlySetProperties.Add("BatchSize");
                        }
                        i++;
                    }
                    break;

                case "--journals":
                case "-j":
                    // Check if next arg is a bool value or just the flag
                    if (nextArg != null && !nextArg.StartsWith("-") && bool.TryParse(nextArg, out var journalsValue))
                    {
                        options.Journals = journalsValue;
                        i++;
                    }
                    else
                    {
                        options.Journals = true;
                    }
                    options.ExplicitlySetProperties.Add("Journals");
                    break;

                case "--keep-journals":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        if (int.TryParse(nextArg, out var keepJournals))
                        {
                            options.KeepJournals = keepJournals;
                            options.ExplicitlySetProperties.Add("KeepJournals");
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

                    // Display score summary
                    if (progress.Score != null)
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold cyan]         Score Summary[/]");
                        AnsiConsole.MarkupLine("         " + new string('-', 43));
                        AnsiConsole.MarkupLine($"         Formula: [yellow]{progress.Score.Formula}[/]");

                        var summary = progress.Score.Summary;
                        if (summary != null)
                        {
                            var passColor = summary.PassingPercent >= 10 ? "green" : summary.PassingPercent >= 5 ? "yellow" : "red";
                            AnsiConsole.MarkupLine($"         Score: [{passColor}]{progress.Score.Value:F0}[/] ({summary.PassingPercent:F1}% of backtests)");
                            AnsiConsole.MarkupLine($"         AD Stats: Max [green]{summary.MaxAd:F2}[/] | Avg [yellow]{summary.AvgAd:F2}[/] | Median {summary.MedianAd:F2}");
                            AnsiConsole.MarkupLine($"         Distribution: [green]{summary.GoodCount}[/] good (≥2) | [cyan]{summary.StrongCount}[/] strong (≥3) | [magenta]{summary.ExceptionalCount}[/] exceptional (≥5)");
                        }

                        // Display histogram
                        var histogram = progress.Score.AdHistogram;
                        if (histogram?.Buckets?.Count > 0)
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLine("[bold cyan]         AD Distribution[/]");
                            var textHistogram = HistogramGenerator.GenerateTextHistogram(histogram, 25);
                            foreach (var line in textHistogram.Split(Environment.NewLine))
                            {
                                // Color buckets based on AD value
                                var colored = line;
                                if (line.Contains("< 0") || line.Contains("-∞"))
                                {
                                    colored = $"[red]{line}[/]";
                                }
                                else if (line.Contains("0.0-") || line.Contains("0.5-"))
                                {
                                    colored = $"[yellow]{line}[/]";
                                }
                                else if (line.Contains("1.0-") || line.Contains("2.0-") || line.Contains("3.0-") || line.Contains("5.0"))
                                {
                                    colored = $"[green]{line}[/]";
                                }
                                AnsiConsole.MarkupLine($"       {colored}");
                            }
                        }
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
        public OptimizationScore? Score { get; set; }
    }
}
