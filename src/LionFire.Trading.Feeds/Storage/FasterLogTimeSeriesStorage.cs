using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FASTER.core;
using LionFire.Trading.Feeds.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Feeds.Storage;

public class FasterLogOptions
{
    public string LogDirectory { get; set; } = "./data/feeds";
    public long SegmentSize { get; set; } = 1024 * 1024 * 1024; // 1GB segments
    public long MemorySize { get; set; } = 1024 * 1024 * 256; // 256MB memory
    public int PageSizeBits { get; set; } = 22; // 4MB pages
}

public class FasterLogTimeSeriesStorage : ITimeSeriesStorage
{
    private readonly FasterLog _log;
    private readonly IDevice _device;
    private readonly ILogger<FasterLogTimeSeriesStorage> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _checkpointDir;

    public FasterLogTimeSeriesStorage(
        IOptions<FasterLogOptions> options,
        ILogger<FasterLogTimeSeriesStorage> logger)
    {
        _logger = logger;
        var opt = options.Value;
        
        Directory.CreateDirectory(opt.LogDirectory);
        _checkpointDir = Path.Combine(opt.LogDirectory, "checkpoints");
        Directory.CreateDirectory(_checkpointDir);

        var logSettings = new FasterLogSettings
        {
            LogDevice = new ManagedLocalStorageDevice(
                Path.Combine(opt.LogDirectory, "feed.log")),
            MemorySizeBits = (int)Math.Log2(opt.MemorySize),
            PageSizeBits = opt.PageSizeBits,
            SegmentSizeBits = (int)Math.Log2(opt.SegmentSize)
        };

        _device = logSettings.LogDevice;
        _log = new FasterLog(logSettings);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task AppendAsync(MarketDataSnapshot snapshot)
    {
        try
        {
            var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            
            await _log.EnqueueAsync(bytes);
            await _log.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append market data snapshot");
            throw;
        }
    }

    public async Task<IEnumerable<MarketDataSnapshot>> ReadRangeAsync(
        string symbol, 
        DateTime startTime, 
        DateTime endTime)
    {
        var results = new List<MarketDataSnapshot>();
        
        try
        {
            using var iter = _log.Scan(_log.BeginAddress, _log.TailAddress);
            
            while (iter.GetNext(out var entry, out _, out _))
            {
                try
                {
                    var json = System.Text.Encoding.UTF8.GetString(entry);
                    var snapshot = JsonSerializer.Deserialize<MarketDataSnapshot>(json, _jsonOptions);
                    
                    if (snapshot != null && 
                        snapshot.Symbol == symbol &&
                        snapshot.Timestamp >= startTime &&
                        snapshot.Timestamp <= endTime)
                    {
                        results.Add(snapshot);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize market data entry");
                }
            }
            
            await iter.WaitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read market data range");
            throw;
        }
        
        return results;
    }

    public async Task FlushAsync()
    {
        await _log.CommitAsync();
    }

    public async Task<long> GetSizeAsync()
    {
        await Task.CompletedTask;
        return _log.TailAddress;
    }

    public void Dispose()
    {
        try
        {
            _log?.Dispose();
            _device?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing FasterLog storage");
        }
    }
}