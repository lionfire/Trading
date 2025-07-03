namespace LionFire.Trading.DataFlow;

public interface IPMarketProcessor  
{
    //IPInput[]? Inputs { get; }

    /// <summary>
    /// Array must match the order of Signals on the bot.
    /// (TODO ENH - Find a way to make this more robust.)
    /// 
    /// If unspecified, assumed to be 0, meaning no lookback.
    /// </summary>
    int[]? InputLookbacks => null;

}

/// <summary>
/// A market processor that iterates at the interval of a particular TimeFrame. (i.e. Bar duration.)
/// </summary>
public interface IPTimeFrameMarketProcessor : IPMarketProcessor
{
    TimeFrame? TimeFrame { get; }

}
