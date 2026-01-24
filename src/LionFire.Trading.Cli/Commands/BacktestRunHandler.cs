using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.CommandLine;
using System.Reflection;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using LionFire.Hosting;
using LionFire.Hosting.CommandLine;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.HistoricalData;
using LionFire.Trading.Journal;

namespace LionFire.Trading.Cli.Commands;

public static class BacktestRunHandler
{
    /// <summary>
    /// Configures command options and adds them to the command
    /// </summary>
    public static void ConfigureCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        // Config file options
        var configOption = new Option<string?>("--config", "Path to HJSON/JSON config file with bot parameters");
        configOption.AddAlias("-c");
        cmd.AddOption(configOption);

        cmd.AddOption(new Option<string?>("--preset", "Named preset from presets directory"));

        // Core options
        var botOption = new Option<string?>("--bot", "Bot type to run (e.g., PAtrBot)");
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

        cmd.AddOption(new Option<bool>("--json", () => false, "Output result as JSON (machine-parseable)"));

        var quietOption = new Option<bool>("--quiet", () => false, "Suppress progress output");
        quietOption.AddAlias("-q");
        cmd.AddOption(quietOption);
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Run =>
        (context, builder) =>
        {
            // Configure services needed for backtest
            builder.Services
                .AutomationModel(builder.Configuration)
                .Automation(builder.Configuration)
                .AddHistoricalBars(builder.Configuration)
                .RegisterTypesFromAssemblies<IPBot2>(typeof(PAtrBot<>))
                .RegisterTypesFromAssemblies<IBot2>(typeof(AtrBot<>))
                .AddSingleton<IBinanceRestClient>(sp => new BinanceRestClient());

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                // Parse args ourselves to track which properties were explicitly set
                var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                var rawOptions = ParseArguments(args);

                // Load config file and presets, merging with command line options
                BacktestRunOptions options;
                try
                {
                    options = BacktestConfigLoader.LoadAndMerge(rawOptions, rawOptions.ExplicitlySetProperties);
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
                        WriteError(options.Json, "Bot type is required. Use --bot <type> or specify in config file");
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

                    // Create the PBot instance
                    var pBot = (IPTimeFrameBot2?)Activator.CreateInstance(pBotType);
                    if (pBot == null)
                    {
                        WriteError(options.Json, $"Failed to create instance of bot type: {pBotType.Name}");
                        Environment.ExitCode = 1;
                        return;
                    }

                    // Set trading parameters on PBot
                    var timeFrame = TimeFrame.Parse(options.Timeframe);
                    var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(options.Exchange, options.Area, options.Symbol, timeFrame);

                    if (pBot is IPBarsBot2 barsBot)
                    {
                        barsBot.ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
                    }

                    // Apply parameters from config if provided
                    if (options.Parameters != null)
                    {
                        PopulateParameters(pBot, options.Parameters);
                    }

                    // Call Init if available (some bots need it)
                    // This sets up derived values and validates parameters
                    if (pBot is IPRequiresInitBeforeUse initBot)
                    {
                        try
                        {
                            initBot.Init();
                        }
                        catch (NullReferenceException ex)
                        {
                            WriteError(options.Json, $"Bot initialization failed - required parameters are missing. " +
                                $"Use --config to provide a config file with bot parameters. " +
                                $"Inner error: {ex.Message}");
                            Environment.ExitCode = 1;
                            return;
                        }
                    }

                    // Snap dates to bar boundaries
                    var snappedStart = timeFrame.GetPeriodStart(options.From);
                    if (snappedStart < options.From)
                    {
                        snappedStart = timeFrame.AddBars(snappedStart, 1);
                    }
                    var snappedEnd = timeFrame.GetPeriodStart(options.To);

                    // Create PMultiSim for single backtest
                    var pMultiSim = new PMultiSim
                    {
                        PBotType = pBotType,
                        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
                        Start = snappedStart,
                        EndExclusive = snappedEnd,
                    };

                    // Set up minimal optimization settings (even for single backtest, we need the infrastructure)
                    pMultiSim.POptimization = new POptimization(pMultiSim)
                    {
                        MaxBacktests = 1,
                        MaxBatchSize = 1,
                        TradeJournalOptions = new TradeJournalOptions
                        {
                            Enabled = true,
                            KeepTradeJournalsForTopNResults = 1,
                        },
                    };

                    // Write starting status
                    WriteStatus(options.Json, options.Quiet, new BacktestResult
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
                    };

                    // Create MultiSimContext for single backtest
                    var dateChunker = serviceProvider.GetRequiredService<DateChunker>();
                    var multiSimContext = ActivatorUtilities.CreateInstance<MultiSimContext>(serviceProvider, pMultiSim, dateChunker);
                    multiSimContext.AddCancellationToken(cts.Token);

                    // Set up OptimizationRunInfo (required even for single backtests)
                    var botType = BotTyping.TryGetBotType(pBotType);
                    multiSimContext.Optimization.OptimizationRunInfo = new OptimizationRunInfo
                    {
                        Guid = multiSimContext.Guid.ToString(),
                        BotName = botType.Name,
                        BotTypeName = botTypeRegistry.GetBotNameFromPBot(pBotType),
                        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
                        Start = snappedStart,
                        EndExclusive = snappedEnd,
                        TicksEnabled = pMultiSim.Features.Ticks(),
                        BotAssemblyNameString = pBotType.Assembly.FullName,
                        OptimizationExecutionDate = DateTime.UtcNow,
                        MachineName = Environment.MachineName,
                    };
                    multiSimContext.Optimization.OptimizationRunInfo.TryHydrateBuildDates(pBotType);

                    await multiSimContext.Init();

                    // Create PBotWrapper for single backtest
                    var wrapper = new PBotWrapper { PBot = pBot };

                    // Create batch and execute
                    var pBatch = new PBatch(multiSimContext, [wrapper]);
                    var batchContext = ActivatorUtilities.CreateInstance<BatchContext<double>>(serviceProvider, multiSimContext, pBatch);
                    var batchHarness = new BatchHarness<double>(batchContext);
                    await batchHarness.Init();

                    // Run the single backtest
                    await batchHarness.StartAsync(cts.Token);
                    await batchHarness.RunTask;

                    stopwatch.Stop();

                    if (cts.Token.IsCancellationRequested)
                    {
                        WriteStatus(options.Json, options.Quiet, new BacktestResult
                        {
                            Status = "cancelled",
                            Timestamp = DateTimeOffset.UtcNow,
                            Message = "Backtest cancelled by user"
                        });
                        Environment.ExitCode = 130;
                        return;
                    }

                    // Get result from journal
                    BacktestBatchJournalEntry? entry = null;
                    if (multiSimContext.Journal?.ObservableCache != null)
                    {
                        entry = multiSimContext.Journal.ObservableCache.Items.FirstOrDefault();
                    }

                    if (entry != null)
                    {
                        var result = new BacktestResult
                        {
                            Status = "completed",
                            Timestamp = DateTimeOffset.UtcNow,
                            Bot = options.Bot,
                            Symbol = options.Symbol,
                            TimeFrame = options.Timeframe,
                            StartDate = options.From.ToString("yyyy-MM-dd"),
                            EndDate = options.To.ToString("yyyy-MM-dd"),
                            AD = Math.Round(entry.AD, 4),
                            Wins = entry.Wins,
                            Losses = entry.Losses,
                            Breakevens = entry.Breakevens,
                            TotalTrades = entry.TotalTrades,
                            WinRate = entry.TotalTrades > 0 ? Math.Round(entry.WinRate * 100, 1) : null,
                            MaxBalanceDrawdownPercent = Math.Round(entry.MaxBalanceDrawdownPerunum * 100, 2),
                            MaxEquityDrawdownPercent = Math.Round(entry.MaxEquityDrawdownPerunum * 100, 2),
                            IsAborted = entry.IsAborted,
                            ElapsedMs = stopwatch.ElapsedMilliseconds,
                        };

                        WriteResult(options.Json, result);
                    }
                    else
                    {
                        WriteError(options.Json, "No results returned from backtest");
                        Environment.ExitCode = 1;
                    }

                    // Clean up
                    if (multiSimContext.Journal != null)
                    {
                        await multiSimContext.Journal.DisposeAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    WriteStatus(options.Json, options.Quiet, new BacktestResult
                    {
                        Status = "cancelled",
                        Timestamp = DateTimeOffset.UtcNow,
                        Message = "Backtest cancelled"
                    });
                    Environment.ExitCode = 130;
                }
                catch (Exception ex)
                {
                    WriteError(options.Json, ex.Message);
                    if (!options.Json)
                    {
                        AnsiConsole.WriteException(ex);
                    }
                    Environment.ExitCode = 1;
                }
            });
        };

    /// <summary>
    /// Populate parameters on a PBot instance from a dictionary of path -> value pairs.
    /// Supports nested properties using dot notation (e.g., "ATR.Period" -> 14)
    /// </summary>
    private static void PopulateParameters(object pBot, Dictionary<string, object> parameters)
    {
        var pBotType = pBot.GetType();

        foreach (var (path, value) in parameters)
        {
            try
            {
                SetValueFromPath(pBot, path, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set parameter '{path}' to '{value}': {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Set a value on an object using a dot-separated property path.
    /// Creates intermediate objects if they don't exist.
    /// </summary>
    private static void SetValueFromPath(object obj, string path, object value)
    {
        var parts = path.Split('.');
        object? current = obj;

        // Navigate to the parent object, creating intermediate objects if needed
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            var property = current!.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new InvalidOperationException($"Property '{part}' not found on type '{current.GetType().Name}'");
            }

            var nextValue = property.GetValue(current);
            if (nextValue == null)
            {
                // Create instance of the property type
                nextValue = Activator.CreateInstance(property.PropertyType);
                property.SetValue(current, nextValue);
            }
            current = nextValue;
        }

        // Set the final property value
        var finalPart = parts[^1];
        var finalProperty = current!.GetType().GetProperty(finalPart, BindingFlags.Public | BindingFlags.Instance);
        if (finalProperty == null)
        {
            throw new InvalidOperationException($"Property '{finalPart}' not found on type '{current.GetType().Name}'");
        }

        var convertedValue = ConvertValue(value, finalProperty.PropertyType);
        finalProperty.SetValue(current, convertedValue);
    }

    /// <summary>
    /// Convert a value to the target type, handling common conversions.
    /// </summary>
    private static object? ConvertValue(object value, Type targetType)
    {
        if (value == null) return null;

        var valueType = value.GetType();
        if (targetType.IsAssignableFrom(valueType)) return value;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle enum conversion from string
        if (underlyingType.IsEnum)
        {
            return Enum.Parse(underlyingType, value.ToString()!, ignoreCase: true);
        }

        // Handle numeric conversions
        if (IsNumericType(underlyingType))
        {
            return Convert.ChangeType(value, underlyingType);
        }

        // Handle string conversion
        if (underlyingType == typeof(string))
        {
            return value.ToString();
        }

        // Default: try Convert.ChangeType
        return Convert.ChangeType(value, underlyingType);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(double)
            || type == typeof(float) || type == typeof(decimal) || type == typeof(short)
            || type == typeof(byte) || type == typeof(uint) || type == typeof(ulong)
            || type == typeof(ushort);
    }

    private static BacktestRunOptions ParseArguments(string[] args)
    {
        var options = new BacktestRunOptions();

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

                case "--json":
                    options.Json = true;
                    options.ExplicitlySetProperties.Add("Json");
                    break;

                case "--quiet":
                case "-q":
                    options.Quiet = true;
                    options.ExplicitlySetProperties.Add("Quiet");
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

    private static void WriteStatus(bool jsonOutput, bool quiet, BacktestResult result)
    {
        if (quiet && result.Status == "starting") return;

        if (jsonOutput)
        {
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        }
        else
        {
            var timestamp = result.Timestamp.ToString("HH:mm:ss");

            switch (result.Status)
            {
                case "starting":
                    AnsiConsole.MarkupLine($"[blue][[{timestamp}]][/] Starting backtest: [cyan]{result.Bot}[/] on [yellow]{result.Symbol}[/] {result.TimeFrame}");
                    AnsiConsole.MarkupLine($"         Date range: {result.StartDate} to {result.EndDate}");
                    break;

                case "cancelled":
                    AnsiConsole.MarkupLine($"[yellow][[{timestamp}]][/] {result.Message}");
                    break;
            }
        }
    }

    private static void WriteResult(bool jsonOutput, BacktestResult result)
    {
        if (jsonOutput)
        {
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        }
        else
        {
            AnsiConsole.WriteLine();
            var panel = new Panel(new Rows(
                new Markup($"[bold]Backtest Results[/] - [cyan]{result.Bot}[/] on [yellow]{result.Symbol}[/] ({result.TimeFrame})"),
                new Markup($"Period: {result.StartDate} to {result.EndDate}"),
                new Rule().RuleStyle(Style.Parse("grey")),
                new Markup($"  [green]AD (Annualized Profit/DD):[/]  [bold]{result.AD:F2}[/]"),
                new Markup($"  Trades:                     {result.TotalTrades} ({result.Wins} wins, {result.Losses} losses, {result.Breakevens} BE)"),
                new Markup($"  Win Rate:                   {result.WinRate:F1}%"),
                new Markup($"  Max Balance Drawdown:       {result.MaxBalanceDrawdownPercent:F1}%"),
                new Markup($"  Max Equity Drawdown:        {result.MaxEquityDrawdownPercent:F1}%"),
                result.IsAborted == true ? new Markup("[red]  (Backtest was aborted)[/]") : new Markup(""),
                new Markup($"  [dim]Elapsed: {result.ElapsedMs}ms[/]")
            ))
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0)
            };

            AnsiConsole.Write(panel);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private class BacktestResult
    {
        public string Status { get; set; } = "";
        public DateTimeOffset Timestamp { get; set; }
        public string? Bot { get; set; }
        public string? Symbol { get; set; }
        public string? TimeFrame { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public double? AD { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Breakevens { get; set; }
        public int? TotalTrades { get; set; }
        public double? WinRate { get; set; }
        public double? MaxBalanceDrawdownPercent { get; set; }
        public double? MaxEquityDrawdownPercent { get; set; }
        public bool? IsAborted { get; set; }
        public long? ElapsedMs { get; set; }
        public string? Message { get; set; }
    }
}
