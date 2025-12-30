using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading;

public enum OhlcAspect
{
    Open,
    High,
    Low,
    Close
}

[Flags]
public enum DataPointAspect
{
    Unspecified = 0,
    Close = 1 << 0,
    Open = 1 << 1,
    High = 1 << 2,
    Low = 1 << 3,
    Volume = 1 << 4,
    //VolumeWeightedClose,
    //WeightedClose ,
    //BidderVolume, // etc.

    /// <summary>
    /// An aspect of the price.
    /// Typically Close, Open, WeightedClose, VolumeWeightedClose, but could also be High, or Low
    /// </summary>
    //Price = 1 << 15,
    //Any = 0xFFFF,
    HLC = High | Low | Close,
    OHLC = Open | High | Low | Close,

    /// <summary>
    /// (High + Low) / 2 - Typical price based on high and low
    /// </summary>
    HL2 = 1 << 5,

    /// <summary>
    /// (High + Low + Close) / 3 - Typical price
    /// </summary>
    HLC3 = 1 << 6,

    /// <summary>
    /// (Open + High + Low + Close) / 4 - Average price
    /// </summary>
    OHLC4 = 1 << 7,
}

public static class DataPointAspectX
{
    public static T GetValue<T>(this DataPointAspect aspect, IKline kline)
    {
        decimal value = aspect.GetValue(kline);

        if (value is T ret) { return ret; }
        else
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
    public static decimal GetValue(this DataPointAspect aspect, IKline kline)
    {
        return aspect switch
        {
            DataPointAspect.Open => kline.OpenPrice,
            DataPointAspect.High => kline.HighPrice,
            DataPointAspect.Low => kline.LowPrice,
            DataPointAspect.Close => kline.ClosePrice,
            DataPointAspect.Volume => kline.Volume,
            DataPointAspect.HL2 => (kline.HighPrice + kline.LowPrice) / 2m,
            DataPointAspect.HLC3 => (kline.HighPrice + kline.LowPrice + kline.ClosePrice) / 3m,
            DataPointAspect.OHLC4 => (kline.OpenPrice + kline.HighPrice + kline.LowPrice + kline.ClosePrice) / 4m,
            //DataPointAspect.HLC => new HLC<decimal>
            //{
            //    High = kline.HighPrice,
            //    Low = kline.LowPrice,
            //    Close = kline.ClosePrice,
            //},
            _ => throw new NotImplementedException(),
        };
    }
}

