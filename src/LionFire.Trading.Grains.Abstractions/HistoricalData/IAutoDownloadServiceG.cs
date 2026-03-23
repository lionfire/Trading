namespace LionFire.Trading.HistoricalData;

[GenerateSerializer]
public class AutoDownloadStatus
{
    [Id(0)]
    public bool IsRunning { get; set; }
    [Id(1)]
    public DateTimeOffset? LastRunStarted { get; set; }
    [Id(2)]
    public DateTimeOffset? LastRunCompleted { get; set; }
    [Id(3)]
    public int LastRunChunksQueued { get; set; }
    [Id(4)]
    public int LastRunChunksSkipped { get; set; }
    [Id(5)]
    public string? LastRunError { get; set; }
}

public interface IAutoDownloadServiceG : IGrainWithStringKey
{
    Task Start();
    Task Stop();
    Task<bool> IsRunning();
    Task RunNow();
    Task<AutoDownloadStatus> GetStatus();
}
