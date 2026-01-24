using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Configuration for file-based symbol collection repository.
/// </summary>
public class FileSymbolCollectionRepositoryOptions
{
    /// <summary>
    /// Base directory for storing collection files.
    /// </summary>
    public string BasePath { get; set; } = "collections";

    /// <summary>
    /// File extension for collection files.
    /// </summary>
    public string FileExtension { get; set; } = ".json";
}

/// <summary>
/// File-based implementation of the symbol collection repository.
/// Stores snapshots as JSON files in a configurable directory.
/// </summary>
public class FileSymbolCollectionRepository : ISymbolCollectionRepository
{
    private readonly ILogger<FileSymbolCollectionRepository> _logger;
    private readonly FileSymbolCollectionRepositoryOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileSymbolCollectionRepository(
        IOptions<FileSymbolCollectionRepositoryOptions> options,
        ILogger<FileSymbolCollectionRepository> logger)
    {
        _options = options?.Value ?? new FileSymbolCollectionRepositoryOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        EnsureDirectoryExists();
    }

    /// <inheritdoc/>
    public async Task SaveAsync(SymbolCollectionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(snapshot.Id);

        try
        {
            var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);

            _logger.LogDebug("Saved collection snapshot {Id} to {Path}", snapshot.Id, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving collection snapshot {Id}", snapshot.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SymbolCollectionSnapshot?> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(id);

        if (!File.Exists(filePath))
        {
            _logger.LogDebug("Collection snapshot {Id} not found at {Path}", id, filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var snapshot = JsonSerializer.Deserialize<SymbolCollectionSnapshot>(json, _jsonOptions);

            _logger.LogDebug("Loaded collection snapshot {Id} from {Path}", id, filePath);
            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading collection snapshot {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SymbolCollectionSnapshot>> ListAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<SymbolCollectionSnapshot>();

        try
        {
            var files = Directory.GetFiles(_options.BasePath, $"*{_options.FileExtension}");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file, cancellationToken);
                    var snapshot = JsonSerializer.Deserialize<SymbolCollectionSnapshot>(json, _jsonOptions);
                    if (snapshot != null)
                    {
                        results.Add(snapshot);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading snapshot from {File}", file);
                }
            }

            _logger.LogDebug("Listed {Count} collection snapshots", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing collection snapshots");
        }

        return results;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(id);

        if (!File.Exists(filePath))
        {
            _logger.LogDebug("Collection snapshot {Id} not found for deletion", id);
            return Task.FromResult(false);
        }

        try
        {
            File.Delete(filePath);
            _logger.LogDebug("Deleted collection snapshot {Id}", id);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting collection snapshot {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(id);
        return Task.FromResult(File.Exists(filePath));
    }

    private string GetFilePath(string id)
    {
        // Sanitize ID to be a valid filename
        var safeId = string.Join("_", id.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_options.BasePath, $"collection-{safeId}{_options.FileExtension}");
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_options.BasePath))
        {
            Directory.CreateDirectory(_options.BasePath);
            _logger.LogInformation("Created collections directory at {Path}", _options.BasePath);
        }
    }
}
