namespace LionFire.Trading;

[Flags]
public enum HedgingKind
{
    Unspecified = 0,
    Hedging = 1 << 0,
    NoHedging = 1 << 1,
}


