namespace LionFire.Trading.ValueWindows;

public interface IReadOnlyValuesWindow
{
    uint Capacity { get; }
    bool IsFull { get; }
    uint Size { get; }

}
public interface IValuesWindow : IReadOnlyValuesWindow
{
    uint TotalValuesSeen { get; }

    void Clear();
}


public interface IValuesWindow<T> : IValuesWindow
{
    IList<ArraySegment<T>> ValuesBuffer { get; }

    T[] ToArray(uint length);
}
