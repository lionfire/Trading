using System.Linq;

namespace LionFire.Trading;

public interface IKlineMarker { }

public interface IKlineWithOpenTime
{
    /// <summary>
    /// The time this candlestick opened
    /// </summary>
    DateTime OpenTime { get; }
}

/// <summary>
/// Kline data
/// </summary>
/// <remarks>
/// Based on IBinanceKline
/// </remarks>
/// <typeparam name="T">Typically supported: decimal, double, float</typeparam>
public interface IKline<T> : IKlineMarker, IKlineWithOpenTime
{


    /// <summary>
    /// The close time of this candlestick
    /// </summary>
    DateTime CloseTime { get; }

    /// <summary>
    /// The amount of trades in this candlestick
    /// </summary>
    int TradeCount { get; }

    /// <summary>
    /// The price at which this candlestick opened
    /// </summary>
    T OpenPrice { get; }

    /// <summary>
    /// The highest price in this candlestick
    /// </summary>
    T HighPrice { get; }

    /// <summary>
    /// The lowest price in this candlestick
    /// </summary>
    T LowPrice { get; }

    /// <summary>
    /// The price at which this candlestick closed
    /// </summary>
    T ClosePrice { get; }

    /// <summary>
    /// The volume traded during this candlestick
    /// </summary>
    T Volume { get; }


    /// <summary>
    /// The volume traded during this candlestick in the asset form
    /// </summary>
    T QuoteVolume { get; }

    /// <summary>
    /// Taker buy base asset volume
    /// </summary>
    T TakerBuyBaseVolume { get; }

    /// <summary>
    /// Taker buy quote asset volume
    /// </summary>
    T TakerBuyQuoteVolume { get; }
}

public interface IKline : IKline<decimal>
{
}

public static class IKlineX
{
    public static bool AreContiguous(this IEnumerable<IKline>? bars, TimeFrame timeFrame, bool noGaps = true)
    {
        if (bars == null || !bars.Any()) return true;

        var last = bars.First().OpenTime;
        foreach (var bar in bars.Skip(1))
        {
            if (noGaps)
            {
                if (timeFrame.TimeSpan <= TimeSpan.Zero) throw new NotImplementedException();
                if (bar.OpenTime != last + timeFrame.TimeSpan)
                {
                    return false;
                }
            }
            else
            {
                if (bar.OpenTime < last)
                {
                    return false;
                }
            }
            last = bar.OpenTime;
        }
        return true;
    }
}

