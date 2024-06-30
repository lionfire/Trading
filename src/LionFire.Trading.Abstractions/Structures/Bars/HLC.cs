#nullable enable

namespace LionFire.Trading;

[ReferenceType(typeof(HLCReference<>))]
public struct HLC<T> : IKlineMarker
{
    public T High { get; set; }
    public T Low { get; set; }
    public T Close { get; set; }
}
