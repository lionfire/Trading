namespace LionFire.Trading.Automation;

public interface IBotContext
{
    long Id { get; }
    IBot2 Bot { get; }

    ValueTask OnFinished();
}
