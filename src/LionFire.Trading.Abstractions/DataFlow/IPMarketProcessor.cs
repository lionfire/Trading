namespace LionFire.Trading.DataFlow;

public interface IPMarketProcessor : IParameters
{
    //IPInput[]? Inputs { get; }
    int[]? InputLookbacks { get; set; } // REVIEW - is there a nicer way to do this?

    Type InstanceType { get; }

}
