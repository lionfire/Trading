using System.IO;
using System.Text.Json;
using System.Threading;
using Hjson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Optimization.Plans.Templates;

/// <summary>
/// Options for the template provider.
/// </summary>
public class TemplateProviderOptions
{
    /// <summary>
    /// Directory containing custom template files.
    /// </summary>
    public string TemplatesDirectory { get; set; } = "";
}

/// <summary>
/// Represents an optimization plan template.
/// </summary>
public record OptimizationPlanTemplate
{
    /// <summary>
    /// Template identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Description of when to use this template.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Category for grouping (e.g., "Coarse Scan", "Fine-Grained", "Quick").
    /// </summary>
    public string Category { get; init; } = "";

    /// <summary>
    /// The template plan configuration.
    /// </summary>
    public OptimizationPlan Plan { get; init; } = new();
}

/// <summary>
/// Provides optimization plan templates.
/// </summary>
public interface IOptimizationPlanTemplateProvider
{
    /// <summary>
    /// Lists all available templates.
    /// </summary>
    Task<IReadOnlyList<OptimizationPlanTemplate>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    Task<OptimizationPlanTemplate?> GetAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of template provider with built-in and file-based templates.
/// </summary>
public class OptimizationPlanTemplateProvider : IOptimizationPlanTemplateProvider
{
    private readonly string _templatesDirectory;
    private readonly ILogger<OptimizationPlanTemplateProvider> _logger;
    private List<OptimizationPlanTemplate>? _cachedTemplates;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OptimizationPlanTemplateProvider(
        IOptions<TemplateProviderOptions> options,
        ILogger<OptimizationPlanTemplateProvider> logger)
    {
        _templatesDirectory = options.Value.TemplatesDirectory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OptimizationPlanTemplate>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedTemplates != null) return _cachedTemplates;

        var templates = new List<OptimizationPlanTemplate>();

        // Add built-in templates
        templates.AddRange(GetBuiltInTemplates());

        // Load file-based templates if directory exists
        if (!string.IsNullOrWhiteSpace(_templatesDirectory) && Directory.Exists(_templatesDirectory))
        {
            foreach (var file in Directory.GetFiles(_templatesDirectory, "*.hjson"))
            {
                try
                {
                    var template = await LoadTemplateFromFile(file, cancellationToken);
                    if (template != null) templates.Add(template);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load template from {File}", file);
                }
            }
        }

        _cachedTemplates = templates;
        return _cachedTemplates;
    }

    public async Task<OptimizationPlanTemplate?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var templates = await ListAsync(cancellationToken);
        return templates.FirstOrDefault(t => t.Id == id);
    }

    private static IEnumerable<OptimizationPlanTemplate> GetBuiltInTemplates()
    {
        yield return new OptimizationPlanTemplate
        {
            Id = "coarse-futures-scan",
            Name = "Coarse Futures Scan",
            Description = "Quick scan across top 50 futures pairs with limited parameter space. Good for initial exploration.",
            Category = "Coarse Scan",
            Plan = new OptimizationPlan
            {
                Id = "",
                Name = "Futures Market Scan",
                Description = "Coarse optimization across top futures markets",
                Bot = "PAtrBot",
                Symbols = new OptimizationPlanSymbols
                {
                    Type = "dynamic",
                    CollectionId = "top-50-futures",
                    Snapshot = []
                },
                Timeframes = ["m5", "m15", "h1", "h4"],
                DateRanges =
                [
                    new() { Name = "1mo", Start = "-1M", End = "now" },
                    new() { Name = "3mo", Start = "-3M", End = "now" },
                    new() { Name = "6mo", Start = "-6M", End = "now" }
                ],
                Resolution = new OptimizationResolution
                {
                    MaxBacktests = 1000,
                    MinParameterPriority = 2
                },
                Tags = ["futures", "coarse", "scan"]
            }
        };

        yield return new OptimizationPlanTemplate
        {
            Id = "fine-grained-single",
            Name = "Fine-Grained Single Symbol",
            Description = "Detailed optimization for a single symbol with full parameter exploration.",
            Category = "Fine-Grained",
            Plan = new OptimizationPlan
            {
                Id = "",
                Name = "Single Symbol Deep Scan",
                Description = "Fine-grained optimization for single symbol",
                Bot = "PAtrBot",
                Symbols = new OptimizationPlanSymbols
                {
                    Type = "static",
                    Snapshot = ["BTCUSDT"]
                },
                Timeframes = ["m5", "m15", "h1"],
                DateRanges =
                [
                    new() { Name = "3mo", Start = "-3M", End = "now" }
                ],
                Resolution = new OptimizationResolution
                {
                    MaxBacktests = 10000,
                    MinParameterPriority = 0
                },
                Tags = ["single", "fine-grained", "deep"]
            }
        };

        yield return new OptimizationPlanTemplate
        {
            Id = "quick-validation",
            Name = "Quick Validation",
            Description = "Fast check across a few symbols to validate strategy viability.",
            Category = "Quick",
            Plan = new OptimizationPlan
            {
                Id = "",
                Name = "Quick Validation Check",
                Description = "Fast strategy validation",
                Bot = "PAtrBot",
                Symbols = new OptimizationPlanSymbols
                {
                    Type = "static",
                    Snapshot = ["BTCUSDT", "ETHUSDT", "BNBUSDT"]
                },
                Timeframes = ["h1"],
                DateRanges =
                [
                    new() { Name = "1mo", Start = "-1M", End = "now" }
                ],
                Resolution = new OptimizationResolution
                {
                    MaxBacktests = 100,
                    MinParameterPriority = 3
                },
                Tags = ["quick", "validation"]
            }
        };

        yield return new OptimizationPlanTemplate
        {
            Id = "multi-timeframe-analysis",
            Name = "Multi-Timeframe Analysis",
            Description = "Test strategy across all common timeframes to find optimal time horizons.",
            Category = "Analysis",
            Plan = new OptimizationPlan
            {
                Id = "",
                Name = "Timeframe Analysis",
                Description = "Compare strategy performance across timeframes",
                Bot = "PAtrBot",
                Symbols = new OptimizationPlanSymbols
                {
                    Type = "static",
                    Snapshot = ["BTCUSDT", "ETHUSDT"]
                },
                Timeframes = ["m1", "m5", "m15", "h1", "h4", "d1"],
                DateRanges =
                [
                    new() { Name = "3mo", Start = "-3M", End = "now" }
                ],
                Resolution = new OptimizationResolution
                {
                    MaxBacktests = 500,
                    MinParameterPriority = 2
                },
                Tags = ["timeframe", "analysis", "comparison"]
            }
        };
    }

    private async Task<OptimizationPlanTemplate?> LoadTemplateFromFile(string path, CancellationToken cancellationToken)
    {
        var hjson = await File.ReadAllTextAsync(path, cancellationToken);
        var json = HjsonValue.Parse(hjson).ToString();
        return JsonSerializer.Deserialize<OptimizationPlanTemplate>(json, JsonOptions);
    }
}
