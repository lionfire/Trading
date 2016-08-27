using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface Symbol
    {
        double Ask { get; }
        double Bid { get; }
        string Code { get; }
        int Digits { get; }
        [Obsolete("Use PreciseLeverage instead")]
        int Leverage { get; }
        long LotSize { get; }
        double PipSize { get; }
        double PipValue { get; }
        [Obsolete("Use TickSize instead")]
        double PointSize { get; }
        double PreciseLeverage { get; }
        double Spread { get; }
        double TickSize { get; }
        double TickValue { get; }
        double UnrealizedGrossProfit { get; }
        double UnrealizedNetProfit { get; }
        long VolumeMax { get; }
        long VolumeMin { get; }
        long VolumeStep { get; }

        long NormalizeVolume(double volume, RoundingMode roundingMode = RoundingMode.ToNearest);
        long QuantityToVolume(double quantity);
        double VolumeToQuantity(long volume);
    }

    //public class SymbolImpl : Symbol
    //{
    //}

}
