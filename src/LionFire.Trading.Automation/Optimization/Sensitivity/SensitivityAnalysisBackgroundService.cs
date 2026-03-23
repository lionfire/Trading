using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Configuration for the background sensitivity analysis service.
/// </summary>
public class SensitivityBackgroundServiceOptions
{
    public const string ConfigurationLocation = "Trading:Optimization:Sensitivity:BackgroundService";

    /// <summary>
    /// Whether the background service is enabled. Default: false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// How often to scan for unanalyzed results. Default: 1 hour.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Maximum number of runs to analyze per poll cycle. Default: 50.
    /// </summary>
    public int MaxPerCycle { get; set; } = 50;
}

/// <summary>
/// Background service that periodically scans for unanalyzed optimization results
/// and runs sensitivity analysis on them.
/// </summary>
public class SensitivityAnalysisBackgroundService : BackgroundService
{
    private readonly BatchSensitivityAnalyzer _batchAnalyzer;
    private readonly IOptionsMonitor<SensitivityBackgroundServiceOptions> _options;
    private readonly ILogger<SensitivityAnalysisBackgroundService> _logger;

    // Track paths that failed analysis to avoid retrying every cycle
    private readonly HashSet<string> _failedPaths = new();

    public SensitivityAnalysisBackgroundService(
        BatchSensitivityAnalyzer batchAnalyzer,
        IOptionsMonitor<SensitivityBackgroundServiceOptions> options,
        ILogger<SensitivityAnalysisBackgroundService> logger)
    {
        _batchAnalyzer = batchAnalyzer;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            _logger.LogInformation("Sensitivity analysis background service is disabled");
            return;
        }

        _logger.LogInformation("Sensitivity analysis background service started, polling every {Interval}",
            _options.CurrentValue.PollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _batchAnalyzer.AnalyzePendingAsync(
                    cancellationToken: stoppingToken);

                if (result.Analyzed > 0)
                {
                    _logger.LogInformation("Background analysis cycle: {Analyzed} analyzed, {Skipped} skipped, {Failed} failed",
                        result.Analyzed, result.Skipped, result.Failed);
                }

                // Track failed paths
                foreach (var error in result.Errors)
                {
                    _failedPaths.Add(error);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sensitivity analysis background service cycle");
            }

            try
            {
                await Task.Delay(_options.CurrentValue.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Sensitivity analysis background service stopped");
    }
}
