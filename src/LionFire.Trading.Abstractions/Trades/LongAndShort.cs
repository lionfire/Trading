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

public static class LongAndShortX
{
    public static LongAndShort Opposite(this LongAndShort longAndShort)
    {
        return longAndShort switch
        {
            LongAndShort.Long => LongAndShort.Short,
            LongAndShort.Short => LongAndShort.Long,
            _ => LongAndShort.Unspecified,
        };
    }
}
