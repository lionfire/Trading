using System.Text.Json;
using Hjson;

namespace LionFire.Trading.Cli.Commands;

/// <summary>
/// Loads optimization configuration from HJSON/JSON files and presets.
/// Precedence: Command line args > Config file > Preset > Defaults
/// </summary>
public static class OptimizeConfigLoader
{
    /// <summary>
    /// Default presets directory. Can be overridden via LFT_PRESETS_DIR environment variable.
    /// </summary>
    public static string PresetsDirectory
    {
        get
        {
            var envDir = Environment.GetEnvironmentVariable("LFT_PRESETS_DIR");
            if (!string.IsNullOrEmpty(envDir)) return envDir;

            // Default: ~/.lft/presets/
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".lft", "presets");
        }
    }

    /// <summary>
    /// Load configuration from a config file and/or preset, merging with command line options.
    /// Command line options always take precedence over config file values.
    /// </summary>
    public static OptimizeRunOptions LoadAndMerge(
        OptimizeRunOptions commandLineOptions,
        HashSet<string> explicitlySetFromCommandLine)
    {
        var result = new OptimizeRunOptions();

        // Step 1: If preset specified, load it first (lowest priority after defaults)
        if (!string.IsNullOrEmpty(commandLineOptions.Preset))
        {
            var presetPath = ResolvePresetPath(commandLineOptions.Preset);
            if (presetPath != null)
            {
                LoadFromFile(presetPath, result);
            }
            else
            {
                throw new FileNotFoundException($"Preset not found: {commandLineOptions.Preset}. Searched in: {PresetsDirectory}");
            }
        }

        // Step 2: If config file specified, load it (overrides preset)
        if (!string.IsNullOrEmpty(commandLineOptions.Config))
        {
            var configPath = ResolveConfigPath(commandLineOptions.Config);
            if (configPath != null)
            {
                LoadFromFile(configPath, result);
            }
            else
            {
                throw new FileNotFoundException($"Config file not found: {commandLineOptions.Config}");
            }
        }

        // Step 3: Apply command line options (highest priority - overrides everything)
        ApplyCommandLineOverrides(commandLineOptions, result, explicitlySetFromCommandLine);

        return result;
    }

    /// <summary>
    /// Resolve preset name to full path. Searches in presets directory.
    /// </summary>
    private static string? ResolvePresetPath(string presetName)
    {
        // Try exact name first
        var candidates = new[]
        {
            Path.Combine(PresetsDirectory, presetName),
            Path.Combine(PresetsDirectory, $"{presetName}.hjson"),
            Path.Combine(PresetsDirectory, $"{presetName}.json"),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }

    /// <summary>
    /// Resolve config path. Can be absolute or relative to current directory.
    /// </summary>
    private static string? ResolveConfigPath(string configPath)
    {
        // If absolute path, use directly
        if (Path.IsPathRooted(configPath))
        {
            return File.Exists(configPath) ? configPath : null;
        }

        // Try relative to current directory
        var fullPath = Path.GetFullPath(configPath);
        if (File.Exists(fullPath)) return fullPath;

        // Try with extensions if not specified
        if (!Path.HasExtension(configPath))
        {
            foreach (var ext in new[] { ".hjson", ".json" })
            {
                var pathWithExt = fullPath + ext;
                if (File.Exists(pathWithExt)) return pathWithExt;
            }
        }

        return null;
    }

    /// <summary>
    /// Load configuration from an HJSON or JSON file into the options object.
    /// </summary>
    private static void LoadFromFile(string filePath, OptimizeRunOptions options)
    {
        var content = File.ReadAllText(filePath);

        // Parse as HJSON (which is a superset of JSON)
        var jsonValue = HjsonValue.Parse(content);
        var jsonString = jsonValue.ToString(Stringify.Plain);
        var config = JsonSerializer.Deserialize<ConfigFile>(jsonString, JsonOptions);

        if (config == null) return;

        // Map config file properties to options
        if (!string.IsNullOrEmpty(config.Bot)) options.Bot = config.Bot;
        if (!string.IsNullOrEmpty(config.Symbol)) options.Symbol = config.Symbol;
        if (!string.IsNullOrEmpty(config.Exchange)) options.Exchange = config.Exchange;
        if (!string.IsNullOrEmpty(config.Area)) options.Area = config.Area;
        if (!string.IsNullOrEmpty(config.ExchangeArea)) options.Area = config.ExchangeArea; // Alias
        if (!string.IsNullOrEmpty(config.Timeframe)) options.Timeframe = config.Timeframe;

        if (config.From.HasValue) options.From = config.From.Value;
        if (config.Start.HasValue) options.From = config.Start.Value; // Alias
        if (config.To.HasValue) options.To = config.To.Value;
        if (config.End.HasValue) options.To = config.End.Value; // Alias

        if (config.ProgressInterval.HasValue) options.ProgressInterval = config.ProgressInterval.Value;
        if (config.Json.HasValue) options.Json = config.Json.Value;
        if (config.Quiet.HasValue) options.Quiet = config.Quiet.Value;
        if (config.MaxBacktests.HasValue) options.MaxBacktests = config.MaxBacktests.Value;
        if (config.BatchSize.HasValue) options.BatchSize = config.BatchSize.Value;

        // Handle nested optimization settings
        if (config.Optimization != null)
        {
            if (config.Optimization.MaxBacktests.HasValue)
                options.MaxBacktests = config.Optimization.MaxBacktests.Value;
            if (config.Optimization.MaxBatchSize.HasValue)
                options.BatchSize = config.Optimization.MaxBatchSize.Value;
            if (config.Optimization.BatchSize.HasValue)
                options.BatchSize = config.Optimization.BatchSize.Value;
        }
    }

    /// <summary>
    /// Apply command line options, but only for properties that were explicitly set.
    /// </summary>
    private static void ApplyCommandLineOverrides(
        OptimizeRunOptions commandLine,
        OptimizeRunOptions target,
        HashSet<string> explicitlySet)
    {
        if (explicitlySet.Contains("Bot") && !string.IsNullOrEmpty(commandLine.Bot))
            target.Bot = commandLine.Bot;
        if (explicitlySet.Contains("Symbol"))
            target.Symbol = commandLine.Symbol;
        if (explicitlySet.Contains("Exchange"))
            target.Exchange = commandLine.Exchange;
        if (explicitlySet.Contains("Area"))
            target.Area = commandLine.Area;
        if (explicitlySet.Contains("Timeframe"))
            target.Timeframe = commandLine.Timeframe;
        if (explicitlySet.Contains("From"))
            target.From = commandLine.From;
        if (explicitlySet.Contains("To"))
            target.To = commandLine.To;
        if (explicitlySet.Contains("ProgressInterval"))
            target.ProgressInterval = commandLine.ProgressInterval;
        if (explicitlySet.Contains("Json"))
            target.Json = commandLine.Json;
        if (explicitlySet.Contains("Quiet"))
            target.Quiet = commandLine.Quiet;
        if (explicitlySet.Contains("MaxBacktests"))
            target.MaxBacktests = commandLine.MaxBacktests;
        if (explicitlySet.Contains("BatchSize"))
            target.BatchSize = commandLine.BatchSize;

        // Always copy config/preset paths
        target.Config = commandLine.Config;
        target.Preset = commandLine.Preset;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Represents the structure of a config file.
    /// Supports multiple aliases for flexibility.
    /// </summary>
    private class ConfigFile
    {
        // Core settings
        public string? Bot { get; set; }
        public string? Symbol { get; set; }
        public string? Exchange { get; set; }
        public string? Area { get; set; }
        public string? ExchangeArea { get; set; } // Alias for Area
        public string? Timeframe { get; set; }

        // Date range (supports multiple names)
        public DateTime? From { get; set; }
        public DateTime? Start { get; set; } // Alias for From
        public DateTime? To { get; set; }
        public DateTime? End { get; set; } // Alias for To

        // Output settings
        public int? ProgressInterval { get; set; }
        public bool? Json { get; set; }
        public bool? Quiet { get; set; }

        // Optimization settings (can be top-level or nested)
        public long? MaxBacktests { get; set; }
        public int? BatchSize { get; set; }

        // Nested optimization settings
        public OptimizationSettings? Optimization { get; set; }

        // Parameter overrides (for future use)
        public Dictionary<string, ParameterRange>? Parameters { get; set; }
    }

    private class OptimizationSettings
    {
        public long? MaxBacktests { get; set; }
        public int? MaxBatchSize { get; set; }
        public int? BatchSize { get; set; }
        public int? MinParameterPriority { get; set; }
    }

    public class ParameterRange
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Step { get; set; }
    }
}
