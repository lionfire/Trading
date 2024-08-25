

namespace LionFire.Trading;

[ReferenceType(typeof(HLCReference<>))]
public struct HLC<T> : IKlineMarker, 
    IClosePrice<T>, IHighLowPrice<T>, IHasPrecision, 
    IPrecision<T>,
    IReferenceTo<T> // REVIEW - redundant to IPrecision<T>?
{
    public T High { get; set; }
    public T Low { get; set; }
    public T Close { get; set; }

    public Type PrecisionType => typeof(T);

    public override string ToString() => string.Format("h:{0} l:{1} c:{2}", High, Low, Close);
}
