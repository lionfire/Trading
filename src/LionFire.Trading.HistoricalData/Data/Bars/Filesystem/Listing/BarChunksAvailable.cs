
using LionFire.Trading.HistoricalData.Sources;

namespace LionFire.Trading.HistoricalData;

public class BarChunksAvailable
{
    public BarsInfo? BarsInfo { get; set; }
    public DateTime First => throw new NotImplementedException();
    public DateTime Last => throw new NotImplementedException();
    public bool IsContiguous => throw new NotImplementedException();
    public List<DateTimeRange> Ranges => throw new NotImplementedException();

    public List<BarsChunkInfo> Chunks { get; } = new();

  }

