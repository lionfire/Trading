using System.Linq;

namespace LionFire.Trading;

/// <summary>
/// Kline data
/// </summary>
/// <remarks>
/// Based on IBinanceKline
/// </remarks>
public interface IKline
{
    /// <summary>
    /// The time this candlestick opened
    /// </summary>
    DateTime OpenTime { get; }

    /// <summary>
    /// The price at which this candlestick opened
    /// </summary>
    decimal OpenPrice { get; }

    /// <summary>
    /// The highest price in this candlestick
    /// </summary>
    decimal HighPrice { get; }

    /// <summary>
    /// The lowest price in this candlestick
    /// </summary>
    decimal LowPrice { get; }

    /// <summary>
    /// The price at which this candlestick closed
    /// </summary>
    decimal ClosePrice { get; }

    /// <summary>
    /// The volume traded during this candlestick
    /// </summary>
    decimal Volume { get; }

    /// <summary>
    /// The close time of this candlestick
    /// </summary>
    DateTime CloseTime { get; }

    /// <summary>
    /// The volume traded during this candlestick in the asset form
    /// </summary>
    decimal QuoteVolume { get; }

    /// <summary>
    /// The amount of trades in this candlestick
    /// </summary>
    int TradeCount { get; }

    /// <summary>
    /// Taker buy base asset volume
    /// </summary>
    decimal TakerBuyBaseVolume { get; }

    /// <summary>
    /// Taker buy quote asset volume
    /// </summary>
    decimal TakerBuyQuoteVolume { get; }
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
                if (bar.OpenTime != last + timeFrame.TimeSpan!.Value)
                {
                    return false;
                }
            }else
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

