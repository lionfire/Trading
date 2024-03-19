namespace LionFire.Trading;

public readonly record struct ReadOnlyTuple<T1,T2>(T1 Item1, T2 Item2) : IReadOnlyTuple<T1, T2>
{
}
