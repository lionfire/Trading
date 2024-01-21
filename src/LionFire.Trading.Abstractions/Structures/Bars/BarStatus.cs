namespace LionFire.Trading;

[Flags]
public enum BarStatus : byte
{
    Unspecified = 0,
    Confirmed = 1 << 0,
    Tentative = 1 << 1,
    Revision = 1 << 2,
    InProgress = 1 << 3,
}

public static class BarStatusX
{
    public static bool IsDone(this BarStatus barStatus) => !barStatus.HasFlag(BarStatus.InProgress);
}
