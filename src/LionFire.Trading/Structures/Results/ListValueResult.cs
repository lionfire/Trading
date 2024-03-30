namespace LionFire.Trading;

public class ListValueResult<T> : IValuesResult<T>
{

    public ListValueResult(IReadOnlyList<T> list)
    {
        List = list;
    }

    public IReadOnlyList<T> List { get; }

    public IReadOnlyList<T> Values => List;
}

