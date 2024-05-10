namespace LionFire.Trading;

public readonly struct ListValueResult<T> : IValuesResult<T>
{

    public ListValueResult(IReadOnlyList<T> list)
    {
        List = list;
    }

    public IReadOnlyList<T> List { get; }

    public IReadOnlyList<T> Values => List;
}

