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
            // TODO: More aspects
            _ => throw new NotImplementedException(),
        };
    }
}

