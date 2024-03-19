namespace LionFire.Trading;

public interface IReadOnlyTuple<out T1, out T2>
{
    T1 Item1 { get; }
    T2 Item2 { get; }
}
