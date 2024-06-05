#nullable enable
namespace LionFire.Trading;

public struct HLC<T>
{
    public T High { get; set; }
    public T Low { get; set; }
    public T Close { get; set; }
}
