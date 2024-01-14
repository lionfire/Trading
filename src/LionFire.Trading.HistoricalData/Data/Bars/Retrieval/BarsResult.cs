
using Binance.Net.Interfaces;
using LionFire.Trading.HistoricalData.Binance;
using System.Collections;

namespace LionFire.Trading.HistoricalData.Retrieval;

// TODO CLEANUP

public interface IBarsResult
{
//    string Name { get; }
//    HistoricalDataSourceKind2 SourceType { get; }
    DateTime EndExclusive { get; init; }
    DateTime Start { get; init; }
    TimeFrame TimeFrame { get; init; }


    IReadOnlyList<IKline> Bars { get; }
    Type NativeType { get; }

    bool IsUpToDate { get; }

    IBarsResult Trim(DateTime start, DateTime endExclusive);
}

public sealed class BarsResult<TKline> : IBarsResult 
    where TKline : IKline
{
    IReadOnlyList<IKline> IBarsResult.Bars => (IReadOnlyList<IKline>)Bars;
    public required IReadOnlyList<TKline> Bars { get; init; }

    public Type NativeType { get => typeof(TKline); }

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

    #region Constructors

    #endregion

    public required TimeFrame TimeFrame { get; init; }
    public required DateTime Start { get; init; }
    public required DateTime EndExclusive { get; init; }

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

    public BarsResult<TKline> Trim(DateTime start, DateTime endExclusive)
    {
        if (start < Start) throw new ArgumentException($"start ({start}) < Start ({Start})");

        int newStartIndex = 0;
        if (start > Start)
        {
            double newStartIndexDouble = (start - Start) / TimeFrame.TimeSpan!.Value;
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
            double newEndIndexExclusiveDouble = (EndExclusive - endExclusive) / TimeFrame.TimeSpan!.Value;
            if (newEndIndexExclusiveDouble % 1.0 != 0.0)
            {
                throw new ArgumentException("End time does not fall on a TimeFrame boundary");
            }
            newEndIndexExclusive = (int)newEndIndexExclusiveDouble;
        }

        return CloneWithBars(this, Bars.Skip(newStartIndex).Take(newEndIndexExclusive - newStartIndex).ToList());
    }
    IBarsResult IBarsResult.Trim(DateTime start, DateTime endExclusive) => Trim(start, endExclusive);

    #endregion

}

