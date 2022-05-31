
namespace LionFire.Trading.HistoricalData;

public class BarsAvailable// : BarsIdentifier
{
    public DateTime First { get; set; }
    public DateTime Last { get; set; }
    public bool IsContiguous { get; set; }
    public List<TimeRange> Ranges { get; set; }

    public List<BarsChunkInfo> Chunks { get; } = new();

    public Task<bool> HasRange(BarsIdentifier id, DateTime start, DateTime endExclusive)
    {
        throw new NotImplementedException();
    }
}

