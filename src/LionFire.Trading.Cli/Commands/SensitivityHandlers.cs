using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using LionFire.Hosting;
using LionFire.Hosting.CommandLine;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Optimization.Sensitivity;

namespace LionFire.Trading.Cli.Commands;

public class RecommendStepsOptions
{
    public string? Plan { get; set; }
    public string? Path { get; set; }
    public string? Metric { get; set; }
    public bool Json { get; set; }
    public bool Quiet { get; set; }
}

public class AnalyzeSensitivityOptions
{
    public string? Plan { get; set; }
    public string? Bot { get; set; }
    public string? Symbol { get; set; }
    public bool All { get; set; }
    public bool Json { get; set; }
    public bool Quiet { get; set; }
}

public static class SensitivityHandlers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    #region recommend-steps

    public static void ConfigureRecommendStepsCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        var pathOption = new Option<string?>("--path", "Path to optimization results directory or zip file");
        pathOption.AddAlias("-p");
        cmd.AddOption(pathOption);

        var planOption = new Option<string?>("--plan", "Plan ID to analyze");
        cmd.AddOption(planOption);

        var metricOption = new Option<string?>("--metric", "Metric to use: AD (default), Fitness");
        metricOption.AddAlias("-m");
        cmd.AddOption(metricOption);

        cmd.AddOption(new Option<bool>("--json", () => false, "Output as JSON"));

        var quietOption = new Option<bool>("--quiet", () => false, "Suppress verbose output");
        quietOption.AddAlias("-q");
        cmd.AddOption(quietOption);
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> RecommendSteps =>
        (context, builder) =>
        {
            builder.Services
                .AddSensitivityAnalysis(builder.Configuration)
                .Configure<BacktestOptions>(builder.Configuration.GetSection(BacktestOptions.ConfigurationLocation));

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var args = System.Environment.GetCommandLineArgs().Skip(1).ToArray();
                var options = ParseRecommendStepsArgs(args);
                var analyzer = serviceProvider.GetRequiredService<SensitivityAnalyzer>();

                if (string.IsNullOrEmpty(options.Path))
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] --path is required (path to optimization results)");
                    System.Environment.ExitCode = 1;
                    return;
                }

                if (!Directory.Exists(options.Path) && !File.Exists(options.Path))
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] Path not found: {options.Path}");
                    System.Environment.ExitCode = 1;
                    return;
                }

                var recommendations = analyzer.Analyze(options.Path, planId: options.Plan);

                if (options.Json)
                {
                    Console.WriteLine(JsonSerializer.Serialize(recommendations, JsonOptions));
                    return;
                }

                if (recommendations.Recommendations.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No recommendations generated[/] (insufficient data or all backtests below trade count threshold)");
                    return;
                }

                // Display as table
                AnsiConsole.MarkupLine($"[bold]Step Recommendations[/] ({recommendations.QualifyingEntries} qualifying entries, metric: {recommendations.Metric})");
                AnsiConsole.WriteLine();

                var table = new Table();
                table.AddColumn("Parameter");
                table.AddColumn(new TableColumn("Sensitivity").RightAligned());
                table.AddColumn(new TableColumn("Rec. Step").RightAligned());
                table.AddColumn(new TableColumn("Rec. Exponent").RightAligned());
                table.AddColumn(new TableColumn("Confidence").RightAligned());
                table.AddColumn("Notes");

                foreach (var rec in recommendations.Recommendations.OrderByDescending(r => r.Sensitivity.Level))
                {
                    var levelColor = rec.Sensitivity.Level switch
                    {
                        SensitivityLevel.High => "red",
                        SensitivityLevel.Medium => "yellow",
                        SensitivityLevel.Low => "green",
                        _ => "white",
                    };

                    var confColor = rec.Confidence >= 0.7 ? "green" : rec.Confidence >= 0.4 ? "yellow" : "red";

                    var notes = new List<string>();
                    if (rec.Sensitivity.HasLocalizedSensitivity) notes.Add("localized");
                    if (rec.Sensitivity.SampleCount < 5) notes.Add("few samples");

                    table.AddRow(
                        rec.ParameterName,
                        $"[{levelColor}]{rec.Sensitivity.Level}[/]",
                        rec.RecommendedStep.ToString("G4"),
                        rec.RecommendedStepExponent.ToString("F1"),
                        $"[{confColor}]{rec.Confidence:P0}[/]",
                        string.Join(", ", notes)
                    );
                }

                AnsiConsole.Write(table);

                // Check for interaction data
                var hessianPersistence = serviceProvider.GetRequiredService<HessianPersistence>();
                var hessianResult = await hessianPersistence.LoadAsync(options.Path);
                if (hessianResult?.Interactions.Any(i => i.Strength != InteractionStrength.Independent) == true)
                {
                    var reporter = serviceProvider.GetRequiredService<InteractionReportGenerator>();
                    var warnings = reporter.GenerateWarnings(hessianResult);
                    if (warnings.Count > 0)
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold yellow]Parameter Interactions:[/]");
                        foreach (var warning in warnings)
                        {
                            AnsiConsole.MarkupLine($"  [yellow]⚠[/] {warning}");
                        }
                    }
                }
            });
        };

    #endregion

    #region analyze-sensitivity

    public static void ConfigureAnalyzeSensitivityCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        cmd.AddOption(new Option<bool>("--all", () => false, "Process all unanalyzed results"));

        var botOption = new Option<string?>("--bot", "Filter by bot type");
        botOption.AddAlias("-b");
        cmd.AddOption(botOption);

        var symbolOption = new Option<string?>("--symbol", "Filter by symbol");
        symbolOption.AddAlias("-s");
        cmd.AddOption(symbolOption);

        cmd.AddOption(new Option<bool>("--json", () => false, "Output as JSON"));

        var quietOption = new Option<bool>("--quiet", () => false, "Suppress verbose output");
        quietOption.AddAlias("-q");
        cmd.AddOption(quietOption);
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> AnalyzeSensitivity =>
        (context, builder) =>
        {
            builder.Services
                .AddSensitivityAnalysis(builder.Configuration)
                .Configure<BacktestOptions>(builder.Configuration.GetSection(BacktestOptions.ConfigurationLocation));

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var args = System.Environment.GetCommandLineArgs().Skip(1).ToArray();
                var options = ParseAnalyzeSensitivityArgs(args);
                var batchAnalyzer = serviceProvider.GetRequiredService<BatchSensitivityAnalyzer>();

                var progress = options.Quiet ? null : new Progress<string>(msg =>
                {
                    AnsiConsole.MarkupLine($"[grey]{msg}[/]");
                });

                var result = await batchAnalyzer.AnalyzePendingAsync(
                    botTypeFilter: options.Bot,
                    symbolFilter: options.Symbol,
                    progress: progress);

                if (options.Json)
                {
                    Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
                    return;
                }

                AnsiConsole.MarkupLine($"[bold]Batch Sensitivity Analysis Complete[/]");
                AnsiConsole.MarkupLine($"  Scanned: {result.TotalScanned}");
                AnsiConsole.MarkupLine($"  Analyzed: [green]{result.Analyzed}[/]");
                AnsiConsole.MarkupLine($"  Skipped: [yellow]{result.Skipped}[/]");
                AnsiConsole.MarkupLine($"  Failed: [red]{result.Failed}[/]");

                if (result.Errors.Count > 0)
                {
                    AnsiConsole.MarkupLine("\n[red]Errors:[/]");
                    foreach (var error in result.Errors.Take(10))
                    {
                        AnsiConsole.MarkupLine($"  [grey]{error}[/]");
                    }
                    if (result.Errors.Count > 10)
                    {
                        AnsiConsole.MarkupLine($"  ... and {result.Errors.Count - 10} more");
                    }
                }
            });
        };

    #endregion

    #region Argument Parsing

    private static RecommendStepsOptions ParseRecommendStepsArgs(string[] args)
    {
        var options = new RecommendStepsOptions();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--path" or "-p" when i + 1 < args.Length:
                    options.Path = args[++i]; break;
                case "--plan" when i + 1 < args.Length:
                    options.Plan = args[++i]; break;
                case "--metric" or "-m" when i + 1 < args.Length:
                    options.Metric = args[++i]; break;
                case "--json":
                    options.Json = true; break;
                case "--quiet" or "-q":
                    options.Quiet = true; break;
            }
        }
        return options;
    }

    private static AnalyzeSensitivityOptions ParseAnalyzeSensitivityArgs(string[] args)
    {
        var options = new AnalyzeSensitivityOptions();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--plan" when i + 1 < args.Length:
                    options.Plan = args[++i]; break;
                case "--bot" or "-b" when i + 1 < args.Length:
                    options.Bot = args[++i]; break;
                case "--symbol" or "-s" when i + 1 < args.Length:
                    options.Symbol = args[++i]; break;
                case "--all":
                    options.All = true; break;
                case "--json":
                    options.Json = true; break;
                case "--quiet" or "-q":
                    options.Quiet = true; break;
            }
        }
        return options;
    }

    #endregion
}
