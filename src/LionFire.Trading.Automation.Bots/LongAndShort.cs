namespace LionFire.Trading.Automation.Bots;

public enum LongAndShort
{
    Unspecified = 0,
    Long = 1 << 0,
    Short = 1 << 1,
    LongAndShort = Long | Short,
}
