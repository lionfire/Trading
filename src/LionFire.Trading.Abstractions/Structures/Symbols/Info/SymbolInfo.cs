using System.Collections.Generic;

namespace LionFire.Trading;

public class SymbolInfo
{
    public string Code { get; set; }
    public int Digits { get; set; }
    public long LotSize { get; set; }
    public double PipSize { get; set; }
    public double PointSize { get; set; }
    public double Leverage { get; set; }
    public double TickSize { get; set; }
    //public double TickValue { get; set; }
    public long VolumeMax { get; set; }
    public long VolumeMin { get; set; }
    public long VolumeStep { get; set; }

    public double VolumeInUnitsMax { get; set; }
    public double VolumeInUnitsMin { get; set; }
    public double VolumeInUnitsStep { get; set; }

    public IReadOnlyList<LeverageTier> DynamicLeverage { get; set; }

    public double VolumePerHundredThousandQuantity { get; set; }
    public double QuantityPerHundredThousandVolume { get; set; }
    public string Currency { get; set; }

}

