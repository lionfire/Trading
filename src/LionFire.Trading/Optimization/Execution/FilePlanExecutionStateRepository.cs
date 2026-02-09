using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Hjson;
using LionFire.Trading.Optimization.Plans;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// File-based repository for persisting plan execution state.
/// </summary>
public class FilePlanExecutionStateRepository : IPlanExecutionStateRepository
{
    private readonly FilePlanExecutionStateOptions _options;
    private readonly ILogger<FilePlanExecutionStateRepository> _logger;
    private readonly string _stateDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public FilePlanExecutionStateRepository(
        IOptions<FilePlanExecutionStateOptions> options,
        ILogger<FilePlanExecutionStateRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
        _stateDirectory = _options.StateDirectory
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LionFire", "Trading", "ExecutionState");

        EnsureDirectoryExists();
    }

    public async Task SaveAsync(PlanExecutionState state, CancellationToken cancellationToken = default)
    {
        var filePath = GetStatePath(state.PlanId);

        _logger.LogDebug("Saving execution state for plan {PlanId} to {Path}", state.PlanId, filePath);

        try
        {
            EnsureDirectoryExists();

            // Serialize to JSON first, then convert to HJSON
            var json = JsonSerializer.Serialize(state, JsonOptions);
            var hjson = JsonValue.Parse(json).ToString(new HjsonOptions { EmitRootBraces = false });

            // Write atomically using temp file
            var tempPath = filePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, hjson, cancellationToken);
            File.Move(tempPath, filePath, overwrite: true);

            _logger.LogInformation(
                "Saved execution state for plan {PlanId}: {Completed}/{Total} jobs complete",
                state.PlanId, state.CompletedJobs, state.TotalJobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save execution state for plan {PlanId}", state.PlanId);
            throw;
        }
    }

    public async Task<PlanExecutionState?> LoadAsync(string planId, CancellationToken cancellationToken = default)
    {
        var filePath = GetStatePath(planId);

        if (!File.Exists(filePath))
        {
            _logger.LogDebug("No execution state found for plan {PlanId}", planId);
            return null;
        }

        try
        {
            _logger.LogDebug("Loading execution state for plan {PlanId} from {Path}", planId, filePath);

            var hjsonContent = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Handle empty or whitespace-only files
            if (string.IsNullOrWhiteSpace(hjsonContent))
            {
                _logger.LogWarning("Execution state file for plan {PlanId} is empty, deleting corrupt file: {Path}", planId, filePath);
                TryDeleteCorruptFile(filePath);
                return null;
            }

            // Add root braces for parsing
            var json = JsonValue.Parse("{" + hjsonContent + "}").ToString(Stringify.Plain);
            var state = JsonSerializer.Deserialize<PlanExecutionState>(json, JsonOptions);

            if (state != null)
            {
                _logger.LogInformation(
                    "Loaded execution state for plan {PlanId}: {Status}, {Completed}/{Total} jobs",
                    planId, state.Status, state.CompletedJobs, state.TotalJobs);
            }

            return state;
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException)
        {
            // File exists but is corrupt - delete it and return null to allow fresh start
            _logger.LogWarning(ex, "Execution state file for plan {PlanId} is corrupt, deleting and returning null: {Path}", planId, filePath);
            TryDeleteCorruptFile(filePath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load execution state for plan {PlanId}", planId);
            throw;
        }
    }

    private void TryDeleteCorruptFile(string filePath)
    {
        try
        {
            // Create backup before deleting
            var backupPath = filePath + ".corrupt." + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            File.Move(filePath, backupPath);
            _logger.LogInformation("Moved corrupt execution state file to: {BackupPath}", backupPath);
        }
        catch (Exception backupEx)
        {
            _logger.LogWarning(backupEx, "Failed to backup corrupt file, attempting direct delete");
            try
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted corrupt execution state file: {Path}", filePath);
            }
            catch (Exception deleteEx)
            {
                _logger.LogError(deleteEx, "Failed to delete corrupt execution state file: {Path}", filePath);
            }
        }
    }

    public Task DeleteAsync(string planId, CancellationToken cancellationToken = default)
    {
        var filePath = GetStatePath(planId);

        if (File.Exists(filePath))
        {
            _logger.LogInformation("Deleting execution state for plan {PlanId}", planId);
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string planId, CancellationToken cancellationToken = default)
    {
        var filePath = GetStatePath(planId);
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<IReadOnlyList<PlanExecutionState>> ListAsync(CancellationToken cancellationToken = default)
    {
        var states = new List<PlanExecutionState>();

        if (!Directory.Exists(_stateDirectory))
        {
            return states;
        }

        foreach (var filePath in Directory.EnumerateFiles(_stateDirectory, "*.hjson"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var planId = Path.GetFileNameWithoutExtension(filePath);
                var state = await LoadAsync(planId, cancellationToken);
                if (state != null)
                {
                    states.Add(state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load state from {Path}", filePath);
            }
        }

        return states;
    }

    private string GetStatePath(string planId)
    {
        // Sanitize planId for use as filename
        var safePlanId = string.Join("_", planId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_stateDirectory, $"{safePlanId}.hjson");
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_stateDirectory))
        {
            Directory.CreateDirectory(_stateDirectory);
        }
    }
}

/// <summary>
/// Options for file-based execution state storage.
/// </summary>
public class FilePlanExecutionStateOptions
{
    /// <summary>
    /// Directory to store execution state files.
    /// Defaults to LocalApplicationData/LionFire/Trading/ExecutionState.
    /// </summary>
    public string? StateDirectory { get; set; }
}
