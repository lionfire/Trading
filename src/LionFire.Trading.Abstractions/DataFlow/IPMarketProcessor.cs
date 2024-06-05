namespace LionFire.Trading.DataFlow;

public interface IPMarketProcessor : IParameters
{
    //IPInput[]? Inputs { get; }
    int[]? InputLookbacks { get; set; } // REVIEW - is there a nicer way to do this?

    Type InstanceType { get; }

}

/// <summary>
/// A market processor that iterates at the interval of a particular TimeFrame. (i.e. Bar duration.)
/// </summary>
public interface IPTimeFrameMarketProcessor : IPMarketProcessor
{
    TimeFrame TimeFrame { get; }

}
