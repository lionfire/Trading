using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LionFire.Trading.Optimization.Matrix;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// File-based repository for persisting plan matrix state as JSON.
/// </summary>
public class FilePlanMatrixStateRepository : IPlanMatrixStateRepository
{
    private readonly FilePlanMatrixStateOptions _options;
    private readonly ILogger<FilePlanMatrixStateRepository> _logger;
    private readonly string _stateDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public FilePlanMatrixStateRepository(
        IOptions<FilePlanMatrixStateOptions> options,
        ILogger<FilePlanMatrixStateRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
        _stateDirectory = _options.StateDirectory
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LionFire", "Trading", "MatrixState");

        EnsureDirectoryExists();
    }

    public async Task SaveAsync(PlanMatrixState state)
    {
        var filePath = GetStatePath(state.PlanId);

        _logger.LogDebug("Saving matrix state for plan {PlanId} to {Path}", state.PlanId, filePath);

        try
        {
            EnsureDirectoryExists();

            var json = JsonSerializer.Serialize(state, JsonOptions);

            var tempPath = filePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, filePath, overwrite: true);

            _logger.LogInformation("Saved matrix state for plan {PlanId}", state.PlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save matrix state for plan {PlanId}", state.PlanId);
            throw;
        }
    }

    public async Task<PlanMatrixState?> LoadAsync(string planId)
    {
        var filePath = GetStatePath(planId);

        if (!File.Exists(filePath))
        {
            _logger.LogDebug("No matrix state found for plan {PlanId}", planId);
            return null;
        }

        try
        {
            _logger.LogDebug("Loading matrix state for plan {PlanId} from {Path}", planId, filePath);

            var json = await File.ReadAllTextAsync(filePath);
            var state = JsonSerializer.Deserialize<PlanMatrixState>(json, JsonOptions);

            if (state != null)
            {
                _logger.LogInformation("Loaded matrix state for plan {PlanId}", planId);
            }

            return state;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load matrix state for plan {PlanId}", planId);
            throw;
        }
    }

    public Task DeleteAsync(string planId)
    {
        var filePath = GetStatePath(planId);

        if (File.Exists(filePath))
        {
            _logger.LogInformation("Deleting matrix state for plan {PlanId}", planId);
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetStatePath(string planId)
    {
        var safePlanId = string.Join("_", planId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_stateDirectory, $"{safePlanId}.matrix-state.json");
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
/// Options for file-based matrix state storage.
/// </summary>
public class FilePlanMatrixStateOptions
{
    /// <summary>
    /// Directory to store matrix state files.
    /// Defaults to LocalApplicationData/LionFire/Trading/MatrixState.
    /// </summary>
    public string? StateDirectory { get; set; }
}
