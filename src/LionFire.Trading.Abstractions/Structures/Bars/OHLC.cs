#nullable enable

namespace LionFire.Trading;

[ReferenceType(typeof(OHLCReference<>))]
public struct OHLC<T> : IKlineMarker, IOpenPrice<T>, IClosePrice<T>, IHighLowPrice<T>
    , IPrecision<T>
{
    public T Open { get; set; }
    public T High { get; set; }
    public T Low { get; set; }
    public T Close { get; set; }

    public override string ToString() => string.Format("o:{0} h:{1} l:{2} c:{3}", Open, High, Low, Close);
}
