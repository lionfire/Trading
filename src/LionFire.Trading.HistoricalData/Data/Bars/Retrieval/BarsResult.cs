namespace LionFire.Trading.HistoricalData.Retrieval;

public class BarsResult
{
    public TimeFrame TimeFrame { get; set; }
    public DateTime Start { get; set; }
    public DateTime EndExclusive { get; set; }
    public Type NativeType { get; set; }

    public IReadOnlyList<object> Bars { get; set; }

    public BarsResult Trim(DateTime start, DateTime endExclusive)
    {
        if(start < Start) throw new ArgumentException($"start ({start}) < Start ({Start})");
        
        int newStartIndex = 0;
        if(start > Start)
        {
            double newStartIndexDouble =  (start-Start) / TimeFrame.TimeSpan.Value;
            if(newStartIndexDouble % 1.0 != 0.0)
            {
                throw new ArgumentException("Start time does not fall on a TimeFrame boundary");
            }
            newStartIndex = (int)newStartIndexDouble;
        }

        int newEndIndexExclusive = Bars.Count;
        if(endExclusive > EndExclusive) { throw new ArgumentException("endExclusive > EndExclusive"); }
        if(endExclusive < EndExclusive)
        {
            double newEndIndexExclusiveDouble = (EndExclusive - endExclusive) / TimeFrame.TimeSpan.Value;
            if (newEndIndexExclusiveDouble % 1.0 != 0.0)
            {
                throw new ArgumentException("End time does not fall on a TimeFrame boundary");
            }
            newEndIndexExclusive = (int)newEndIndexExclusiveDouble;
        }

        return new BarsResult
        {
            TimeFrame = TimeFrame,
            Start = start,
            EndExclusive = endExclusive,
            Bars = Bars.Skip(newStartIndex).Take(newEndIndexExclusive - newStartIndex).ToList(),
        };
    }
}

