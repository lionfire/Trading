
using Binance.Net.Interfaces;
using LionFire.Trading.HistoricalData.Binance;
using System.Collections;

namespace LionFire.Trading.HistoricalData.Retrieval;

// TODO CLEANUP and REVIEW

public interface IBarsResult : ITimeSeriesResult<IKline>
{
    //    string Name { get; }
    //    HistoricalDataSourceKind2 SourceType { get; }

    IReadOnlyList<IKline> Bars => Values;
    Type NativeType { get; }

    bool IsUpToDate { get; }

    IBarsResult Trim(DateTimeOffset start, DateTimeOffset endExclusive);
}

public sealed class BarsResult<TKline> : IBarsResult
    where TKline : IKline
{

    #region Values

    //IReadOnlyList<IKline> IBarsResult.Bars => (IReadOnlyList<IKline>)Bars;
    IReadOnlyList<IKline> IValuesResult<IKline>.Values => (IReadOnlyList<IKline>)Bars;

    public required IReadOnlyList<TKline> Bars { get; init; }
    //public IEnumerable<IKline> Values => Bars.OfType<IKline>();

    #endregion

    public Type NativeType { get => typeof(TKline); }


    #region Constructors

    private BarsResult<TKline> CloneWithBars(IBarsResult barsResult, IReadOnlyList<TKline> bars)
    {
        return new BarsResult<TKline>
        {
            TimeFrame = barsResult.TimeFrame,
            Start = barsResult.Start,
            EndExclusive = barsResult.EndExclusive,
            Bars = bars,
        };
    }

    #endregion

    public required TimeFrame TimeFrame { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset EndExclusive { get; init; }

    //public required DateTime LastBarOpenTime { get; init; }
    //public required DateTime LastBarCloseTime { get; init; }

    #region (derived) State

    /// <summary>
    /// Considered up to date if IsForeverUpToDate, or if the bar that is in progress hasn't finished yet.
    /// 
    /// Note: this can become false over time.  It never goes back to true.
    /// 
    /// </summary>
    public bool IsUpToDate => Bars.Count == TimeFrame.GetExpectedBarCountForNow(Start, EndExclusive);

    //IsForeverUpToDate || DateTime.UtcNow < (LastBarCloseTime + TimeFrame.TimeSpan!.Value);

    ///// <summary>
    ///// Note: this can become true over time.  It never goes back to false.
    ///// </summary>
    //public bool IsForeverUpToDate => EndExclusive >= DateTime.UtcNow && Bars.Count == TimeFrame.GetExpectedBarCountForNow(Start, EndExclusive);

    #endregion

    #region Methods

    public BarsResult<TKline> Trim(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        if (start < Start) throw new ArgumentException($"start ({start}) < Start ({Start})");
        if (TimeFrame.TimeSpan <= TimeSpan.Zero) throw new NotImplementedException();
        int newStartIndex = 0;
        if (start > Start)
        {
            double newStartIndexDouble = (start - Start) / TimeFrame.TimeSpan;
            if (newStartIndexDouble % 1.0 != 0.0)
            {
                throw new ArgumentException("Start time does not fall on a TimeFrame boundary");
            }
            newStartIndex = (int)newStartIndexDouble;
        }

        int newEndIndexExclusive = Bars.Count;
        if (endExclusive > EndExclusive) { throw new ArgumentException("endExclusive > EndExclusive"); }
        if (endExclusive < EndExclusive)
        {
            if (TimeFrame.TimeSpan < TimeSpan.Zero) throw new NotImplementedException();
            double newEndIndexExclusiveDouble = (EndExclusive - endExclusive) / TimeFrame.TimeSpan;
            if (newEndIndexExclusiveDouble % 1.0 != 0.0)
            {
                throw new ArgumentException("End time does not fall on a TimeFrame boundary");
            }
            newEndIndexExclusive = (int)newEndIndexExclusiveDouble;
        }

        return CloneWithBars(this, Bars.Skip(newStartIndex).Take(newEndIndexExclusive - newStartIndex).ToList());
    }
    IBarsResult IBarsResult.Trim(DateTimeOffset start, DateTimeOffset endExclusive) => Trim(start, endExclusive);

    #endregion

}

