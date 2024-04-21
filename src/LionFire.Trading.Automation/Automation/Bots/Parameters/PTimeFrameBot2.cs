namespace LionFire.Trading.Automation;

public class PTimeFrameBot2<TConcrete> : PBot2<TConcrete>
{
    public required TimeFrame TimeFrame { get; set; }

    /// <summary>
    /// If Ticks is false, Update the bot this many times per bar.  (Default: 1)
    /// </summary>
    public int UpdatesPerBar { get; set; } = 1;
    public bool Ticks { get; set; }
}
