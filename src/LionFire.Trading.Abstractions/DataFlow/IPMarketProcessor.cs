namespace LionFire.Trading.DataFlow;

public interface IPMarketProcessor : IParameters
{
    //IPInput[]? Inputs { get; }
    int[]? InputLookbacks { get; set; }

    Type InstanceType { get; }

}
