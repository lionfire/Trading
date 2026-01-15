// Type aliases for common value types used in tests
// This provides convenient non-generic names for common HLC configurations

namespace LionFire.Trading.ValueTypes;

/// <summary>
/// HLC with double precision - the most common type for indicator tests
/// </summary>
public struct HLC
{
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }

    public static implicit operator HLC<double>(HLC hlc) => new()
    {
        High = hlc.High,
        Low = hlc.Low,
        Close = hlc.Close
    };

    public static implicit operator HLC(HLC<double> hlc) => new()
    {
        High = hlc.High,
        Low = hlc.Low,
        Close = hlc.Close
    };
}

/// <summary>
/// OHLC with double precision
/// </summary>
public struct OHLC
{
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
}

/// <summary>
/// OHLCV (with Volume) with double precision
/// </summary>
public struct OHLCV
{
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public double Volume { get; set; }
}

/// <summary>
/// HLCV (High, Low, Close, Volume) with double precision
/// </summary>
public struct HLCV
{
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public double Volume { get; set; }
}

/// <summary>
/// HL (High, Low) with double precision
/// </summary>
public struct HL
{
    public double High { get; set; }
    public double Low { get; set; }

    public static implicit operator HL<double>(HL hl) => new()
    {
        High = hl.High,
        Low = hl.Low
    };

    public static implicit operator HL(HL<double> hl) => new()
    {
        High = hl.High,
        Low = hl.Low
    };
}

/// <summary>
/// PriceVolume with double precision for VWMA indicators
/// </summary>
public struct PriceVolume
{
    public double Price { get; set; }
    public double Volume { get; set; }
}
