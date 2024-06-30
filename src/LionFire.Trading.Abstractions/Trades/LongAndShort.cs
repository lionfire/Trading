namespace LionFire.Trading;

[Optimizable]
public enum LongAndShort
{
    Unspecified = 0,

    [Optimizable]
    Long = 1 << 0,
    [Optimizable]
    Short = 1 << 1,
    [Optimizable]
    LongAndShort = Long | Short,
}
