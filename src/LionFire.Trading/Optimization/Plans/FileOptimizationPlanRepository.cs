using System.IO;
using System.Text.Json;
using System.Threading;
using Hjson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Options for the file-based plan repository.
/// </summary>
public class OptimizationPlanRepositoryOptions
{
    /// <summary>
    /// Root directory for plan storage.
    /// </summary>
    public string PlansDirectory { get; set; } = "";
}

/// <summary>
/// File-based repository for optimization plans using HJSON format.
/// </summary>
public class FileOptimizationPlanRepository : IOptimizationPlanRepository
{
    private readonly string _plansDirectory;
    private readonly ILogger<FileOptimizationPlanRepository> _logger;
    private readonly OptimizationPlanValidator _validator;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public FileOptimizationPlanRepository(
        IOptions<OptimizationPlanRepositoryOptions> options,
        OptimizationPlanValidator validator,
        ILogger<FileOptimizationPlanRepository> logger)
    {
        _plansDirectory = options.Value.PlansDirectory;
        _validator = validator;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_plansDirectory) && !Directory.Exists(_plansDirectory))
        {
            Directory.CreateDirectory(_plansDirectory);
            _logger.LogInformation("Created plans directory: {Directory}", _plansDirectory);
        }
    }

    public async Task<OptimizationPlan?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var path = GetPlanPath(id);
        if (!File.Exists(path)) return null;

        try
        {
            var hjson = await File.ReadAllTextAsync(path, cancellationToken);
            return DeserializePlan(hjson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load plan {Id} from {Path}", id, path);
            return null;
        }
    }

    public async Task<IReadOnlyList<OptimizationPlan>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_plansDirectory) || !Directory.Exists(_plansDirectory))
        {
            return [];
        }

        var plans = new List<OptimizationPlan>();
        var files = Directory.GetFiles(_plansDirectory, "*.hjson");

        foreach (var file in files)
        {
            try
            {
                var hjson = await File.ReadAllTextAsync(file, cancellationToken);
                var plan = DeserializePlan(hjson);
                if (plan != null) plans.Add(plan);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load plan from {File}", file);
            }
        }

        return plans.OrderBy(p => p.Name).ToList();
    }

    public async Task<OptimizationPlan> SaveAsync(OptimizationPlan plan, CancellationToken cancellationToken = default)
    {
        // Validate
        var validation = _validator.Validate(plan);
        if (!validation.IsValid)
        {
            throw new PlanValidationException(validation.Errors);
        }

        // Log warnings
        foreach (var warning in validation.Warnings)
        {
            _logger.LogWarning("Plan validation warning: {Warning}", warning);
        }

        // Check if updating existing
        var existing = await GetAsync(plan.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var planToSave = existing != null
            ? plan with
            {
                Version = existing.Version + 1,
                Created = existing.Created,
                Modified = now
            }
            : plan with
            {
                Version = 1,
                Created = now,
                Modified = now
            };

        // Serialize and save
        var hjson = SerializePlan(planToSave);
        var path = GetPlanPath(planToSave.Id);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, hjson, cancellationToken);

        _logger.LogInformation("Saved plan {Id} (version {Version}) to {Path}",
            planToSave.Id, planToSave.Version, path);
        return planToSave;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var path = GetPlanPath(id);
        if (!File.Exists(path)) return Task.FromResult(false);

        File.Delete(path);
        _logger.LogInformation("Deleted plan {Id}", id);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(GetPlanPath(id)));
    }

    private string GetPlanPath(string id) =>
        Path.Combine(_plansDirectory, $"{id}.hjson");

    private static OptimizationPlan? DeserializePlan(string hjson)
    {
        var json = HjsonValue.Parse(hjson).ToString();
        return JsonSerializer.Deserialize<OptimizationPlan>(json, JsonOptions);
    }

    private static string SerializePlan(OptimizationPlan plan)
    {
        var json = JsonSerializer.Serialize(plan, JsonOptions);
        return JsonValue.Parse(json).ToString(Stringify.Hjson);
    }
}
