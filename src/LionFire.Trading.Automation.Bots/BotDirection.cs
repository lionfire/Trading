namespace LionFire.Trading.Automation.Bots;

public enum BotDirection
{
    Unspecified = 0,
    Unidirectional = 1 << 0,
    Bidirectional = 1 << 1,
}
