
namespace LionFire.Trading.HistoricalData;

public class BarsChunkInfo
{
    public string ChunkName { get; set; }
    public DateTime Start { get; set; }
    public DateTime EndExclusive { get; set; }
    public long? ExpectedBars { get; set; }
    public long? Bars { get; set; }
    public object Percent { get; set; }
}

