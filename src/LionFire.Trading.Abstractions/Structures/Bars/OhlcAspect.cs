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
}

public static class DataPointAspectX
{
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

