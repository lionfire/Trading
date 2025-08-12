namespace LionFire.Trading;

[ReferenceType(typeof(HLReference<>))]
public struct HL<T> : IKlineMarker, 
    IHighLowPrice<T>, IHasPrecision, 
    IPrecision<T>,
    IReferenceTo<T> // REVIEW - redundant to IPrecision<T>?
{
    public T High { get; set; }
    public T Low { get; set; }

    public Type PrecisionType => typeof(T);

    public override string ToString() => string.Format("h:{0} l:{1}", High, Low);
}