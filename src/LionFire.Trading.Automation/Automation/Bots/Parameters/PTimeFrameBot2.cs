namespace LionFire.Trading.Automation;


public interface IPTimeFrameBot2 : IPBot2, IPTimeFrameMarketProcessor
{
}

public abstract class PTimeFrameBot2<TConcrete> : PStandardBot2<TConcrete>, IPTimeFrameBot2
    where TConcrete : PTimeFrameBot2<TConcrete>
{
    //public PTimeFrameBot2() { }
    public PTimeFrameBot2(TimeFrame timeFrame)
    {
        TimeFrame = timeFrame;
    }

    public TimeFrame TimeFrame { get; init; }

#if ENH
    /// <summary>
    /// If Ticks is false, Update the bot this many times per bar.  (Default: 1)
    /// Bot will be notified using a simulated Tick that represents a portion of the bar
    /// </summary>
    public int UpdatesPerBar { get; set; } = 1;
#endif
}
