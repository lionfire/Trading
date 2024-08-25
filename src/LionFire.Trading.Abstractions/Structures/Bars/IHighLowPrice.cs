

namespace LionFire.Trading;

public interface IHighLowPrice<T>
{
    T High { get; }
    T Low { get; }
}
